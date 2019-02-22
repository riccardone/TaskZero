using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using TaskZero.Adapter;
using TaskZero.ReadModels.InMemory;
using TaskZero.Repository.EventStore;

namespace TaskZero.Console
{
    class Program
    {
        private static Uri _uri;

        static void Main(string[] args)
        {
            var es = "localhost:1113";
            if (args.Length > 0)
                es = args[0];
            _uri = new Uri($"tcp://{es}");
            try
            {
                var synchroniserService = new SynchroniserService(BuildConnection("es-taskzero-syncroniser", _uri),
                    new UserCredentials("admin", "changeit"));
                var domainConnection = BuildConnection("es-taskzero-domain", _uri);
                domainConnection.ConnectAsync().Wait();
                var domainRepository = new EventStoreDomainRepository("domain", domainConnection);
                var handler = new Handler(domainRepository);
                var worker = new Worker("TaskZero.Console", synchroniserService, handler);
                worker.Run();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            System.Console.WriteLine("Press enter to exit");
            System.Console.ReadLine();
        }

        private static IEventStoreConnection BuildConnection(string name, Uri uri)
        {
            return EventStoreConnection.Create(ConnectionSettings.Create().KeepRetrying().KeepReconnecting(), uri, name);
        }
    }
}
