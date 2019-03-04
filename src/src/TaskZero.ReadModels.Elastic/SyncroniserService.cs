using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json.Linq;
using TaskZero.ReadModels.Elastic.Model;

namespace TaskZero.ReadModels.Elastic
{
    public class SyncroniserService
    {
        // This is responsible to keep up-to-date the readmodel in an Elastic Search index

        private readonly IEventStoreConnection _conn;
        private readonly UserCredentials _credentials;
        private readonly IIndexer<ZeroTask> _indexer;
        public event EventHandler LiveSynchStarted;

        public SyncroniserService(IEventStoreConnection conn, UserCredentials credentials, IIndexer<ZeroTask> indexer)
        {
            _conn = conn;
            _credentials = credentials;
            _indexer = indexer;
        }

        public async Task Start()
        {
            _conn.Reconnecting += _conn_Reconnecting;
            _conn.Disconnected += _conn_Disconnected;
            _conn.Connected += _conn_Connected;
            await _conn.ConnectAsync();
        }

        private void _conn_Connected(object sender, ClientConnectionEventArgs e)
        {
            SubscribeMe();
        }

        private void SubscribeMe()
        {
            //_conn.SubscribeToStreamFrom("$ce-domain", StreamCheckpoint.StreamStart, CatchUpSubscriptionSettings.Default,
            //    EventAppeared, LiveProcessingStarted, SubscriptionDropped, _credentials);
            _conn.SubscribeToAllFrom(Position.Start, CatchUpSubscriptionSettings.Default,
                EventAppeared, LiveProcessingStarted, SubscriptionDropped, _credentials);
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription obj)
        {
            OnLiveProcessingStarted(new EventArgs());
        }

        private Task EventAppeared(EventStoreCatchUpSubscription arg1, ResolvedEvent arg2)
        {
            if (arg2.Event == null || arg2.Event.EventStreamId.StartsWith("$") || arg2.Event.EventType.StartsWith("$"))
                return Task.CompletedTask;

            dynamic data = JObject.Parse(Encoding.UTF8.GetString(arg2.Event.Data));
            dynamic metaData = JObject.Parse(Encoding.UTF8.GetString(arg2.Event.Metadata));

            if (arg2.Event.EventType.Equals("TaskAddedV1"))
                HandleTaskAdded(data, metaData);
            if (arg2.Event.EventType.Equals("TaskRemovedV1"))
                HandleTaskDeleted(data, metaData);
            if (arg2.Event.EventType.Equals("WrongRemoveTaskRequestedV1"))
                HandleWrongRemoveTaskRequested(data, metaData);

            return Task.CompletedTask;
        }

        private void _conn_Disconnected(object sender, ClientConnectionEventArgs e)
        {
            Console.WriteLine("Disconnected...");
        }

        private void _conn_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            Console.WriteLine("Reconnecting to EventStore...");
        }

        private void SubscriptionDropped(EventStoreCatchUpSubscription arg1, SubscriptionDropReason arg2,
            Exception arg3)
        {
            SubscribeMe();
        }

        protected virtual void OnLiveProcessingStarted(EventArgs e)
        {
            LiveSynchStarted?.Invoke(this, e);
        }

        private void HandleWrongRemoveTaskRequested(dynamic data, dynamic metaData)
        {
            // Log this info in a event index?
        }

        private void HandleTaskDeleted(dynamic data, dynamic metaData)
        {
            _indexer.Remove(data.TaskToDeleteId.Value);
        }

        private void HandleTaskAdded(dynamic data, dynamic metaData)
        {
            DateTime? dueDate = null;

            if (data.DueDate.Value != null)
                if (DateTime.TryParse(data.DueDate.Value.ToString(), out DateTime dueDateVal))
                    dueDate = dueDateVal;

            var zeroTask = new ZeroTask(data.Id.Value, metaData.username.Value, data.Title.Value, data.Description.Value, dueDate,
                ((Priority) data.Priority.Value).ToString(), metaData.source.Value, metaData["$correlationId"].Value, metaData["applies"].Value);

            _indexer.Index(zeroTask);
        }
    }
}