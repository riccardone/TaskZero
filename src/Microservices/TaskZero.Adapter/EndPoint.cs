using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using NLog;
using TaskZero.Adapter.WeakSchemaMappings;
using TaskZero.Domain;
using TaskZero.Domain.Messages.Commands;

namespace TaskZero.Adapter
{
    public class EndPoint
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IEventStoreConnection _subscriberConnection;
        private readonly IDomainRepository _repo;
        private const string InputStream = "input-zerotask";
        private const string PersistentSubscriptionGroup = "zerotask-processors";
        private readonly Dictionary<string, Func<string[], Command>> _deserialisers;
        private readonly Dictionary<string, Func<object, IAggregate>> _eventHandlerMapping;
        private readonly Handler _handler;
        private readonly UserCredentials _credentials;

        public EndPoint(IEventStoreConnection subscriberConnection, IDomainRepository repo, Handler handler, UserCredentials credentials)
        {
            _deserialisers = CreateDeserialisersMapping();
            _eventHandlerMapping = CreateEventHandlerMapping();
            _subscriberConnection = subscriberConnection;
            _repo = repo;
            _handler = handler;
            _credentials = credentials;
        }

        public bool Start()
        {
            try
            {
                _subscriberConnection.Connected += _connection_Connected;
                _subscriberConnection.Disconnected += _connection_Disconnected;
                _subscriberConnection.ErrorOccurred += _connection_ErrorOccurred;
                _subscriberConnection.Closed += _connection_Closed;
                _subscriberConnection.Reconnecting += _connection_Reconnecting;
                _subscriberConnection.AuthenticationFailed += _connection_AuthenticationFailed;
                _subscriberConnection.ConnectAsync();
                Log.Info($"Listening from '{InputStream}' stream");
                Log.Info($"Joined '{PersistentSubscriptionGroup}' group");
                Log.Info("Log EndPoint started");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        private void _connection_AuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs e)
        {
            Log.Error($"EndpointConnection AuthenticationFailed: {e.Reason}");
        }

        private void _connection_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            Log.Warn($"EndpointConnection Reconnecting...");
        }

        private void _connection_Closed(object sender, ClientClosedEventArgs e)
        {
            Log.Info($"EndpointConnection Closed: {e.Reason}");
        }

        private async Task CreateSubscription()
        {
            await _subscriberConnection.CreatePersistentSubscriptionAsync(InputStream, PersistentSubscriptionGroup,
                PersistentSubscriptionSettings.Create().StartFromBeginning().DoNotResolveLinkTos(),
                _credentials);
        }

        private static void _connection_ErrorOccurred(object sender, ClientErrorEventArgs e)
        {
            Log.Error($"EndpointConnection ErrorOccurred: {e.Exception.Message}");
        }

        private static void _connection_Disconnected(object sender, ClientConnectionEventArgs e)
        {
            Log.Error($"EndpointConnection Disconnected from {e.RemoteEndPoint}");
        }

        private async void _connection_Connected(object sender, ClientConnectionEventArgs e)
        {
            Log.Info($"EndpointConnection Connected to {e.RemoteEndPoint}");
            try
            {
                await CreateSubscription();
            }
            catch (Exception)
            {
                // already exist
            }
            await Subscribe();
        }

        private async Task Subscribe()
        {
            await _subscriberConnection.ConnectToPersistentSubscriptionAsync(InputStream, PersistentSubscriptionGroup, EventAppeared, SubscriptionDropped);
        }

        private Task EventAppeared(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase, ResolvedEvent resolvedEvent)
        {
            try
            {
                Process(resolvedEvent.Event.EventType, resolvedEvent.Event.Metadata, resolvedEvent.Event.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                eventStorePersistentSubscriptionBase.Fail(resolvedEvent, PersistentSubscriptionNakEventAction.Park,
                    ex.GetBaseException().Message);
            }
            return Task.CompletedTask;
        }

        private void Process(string eventType, byte[] metadata, byte[] data)
        {
            if (!_deserialisers.ContainsKey(eventType))
                return;

            var command = _deserialisers[eventType](new[]
            {
                Encoding.UTF8.GetString(metadata),
                Encoding.UTF8.GetString(data)
            });

            if (command == null)
            {
                Log.Error($"Message format not recognised! EventType: {eventType}");
                return;
            }

            foreach (var key in _eventHandlerMapping.Keys)
            {
                if (!eventType.EndsWith(key))
                    continue;
                var aggregate = _eventHandlerMapping[key](command);
                _repo.Save(aggregate).Wait();
                Log.Debug($"Handled '{eventType}' AggregateId: {aggregate.AggregateId}");
                return;
            }
            throw new Exception($"I can't find any handler for {eventType}");
        }

        private static void SubscriptionDropped(EventStorePersistentSubscriptionBase eventStorePersistentSubscriptionBase, SubscriptionDropReason subscriptionDropReason, Exception arg3)
        {
            Log.Error(arg3, subscriptionDropReason.ToString());
        }

        private static Dictionary<string, Func<string[], Command>> CreateDeserialisersMapping()
        {
            return new Dictionary<string, Func<string[], Command>>
            {
                {"AddNewTask", ToAddNewTask},
                {"RemoveTask", ToRemoveTask}
            };
        }

        private Dictionary<string, Func<object, IAggregate>> CreateEventHandlerMapping()
        {
            return new Dictionary<string, Func<object, IAggregate>>
            {
                {"AddNewTask", o => _handler.Handle(o as AddNewTask)},
                {"RemoveTask", o => _handler.Handle(o as RemoveTask)}
            };
        }

        private static Command ToAddNewTask(string[] arg)
        {
            return new AddNewTaskFromJson(arg[1], arg[0]);
        }

        private static Command ToRemoveTask(string[] arg)
        {
            return new RemoveTaskFromJson(arg[1], arg[0]);
        }
    }
}
