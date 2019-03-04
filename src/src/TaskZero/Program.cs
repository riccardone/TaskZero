using System;
using Nest;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using TaskZero.Adapter;
using TaskZero.Repository.EventStore;

namespace TaskZero
{
    class Program
    {
        private static Uri _eventStoreUri;
        private static Uri _elasticSearchUri;

        static void Main(string[] args)
        {
            try
            {
                var eventStore = "tcp://localhost:1113";
                if (args.Length > 0)
                    eventStore = args[0];
                _eventStoreUri = new Uri(eventStore);
                if (args.Length > 1)
                    _elasticSearchUri = new Uri(args[1]);

                var inMemorySynchronizer = new ReadModels.InMemory.SyncroniserService(BuildConnection("taskzero-synchronizer-inmemory", _eventStoreUri),
                    new UserCredentials("admin", "changeit"));
                ReadModels.Elastic.SyncroniserService elasticSearchSynchronizer = null;
                if (_elasticSearchUri != null)
                    elasticSearchSynchronizer = new ReadModels.Elastic.SyncroniserService(BuildConnection("taskzero-synchronizer-elastic", _eventStoreUri),
                        new UserCredentials("admin", "changeit"), new ReadModels.Elastic.Indexer<ReadModels.Elastic.Model.ZeroTask>(2000,
                            new ElasticClient(_elasticSearchUri), "taskzero-tasks"));
                var commandsHandler =
                    new Handler(new EventStoreDomainRepository("domain",
                        BuildConnection("es-taskzero-domain", _eventStoreUri, true)));
                new Worker("TaskZero.Console", inMemorySynchronizer, elasticSearchSynchronizer, commandsHandler).Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetBaseException().Message);
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static IEventStoreConnection BuildConnection(string name, Uri uri, bool openConnection = false)
        {
            var conn = EventStoreConnection.Create(
                EventStore.ClientAPI.ConnectionSettings.Create().KeepRetrying().KeepReconnecting(), uri, name);
            if (openConnection)
                conn.ConnectAsync().Wait();
            return conn;
        }
    }
}
