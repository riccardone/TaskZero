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
        private static Uri _uri;

        static void Main(string[] args)
        {
            var es = "localhost:1113";
            if (args.Length > 0)
                es = args[0];
            _uri = new Uri($"tcp://{es}");
            try
            {
                var inMemorySynchronizer = new ReadModels.InMemory.SyncroniserService(BuildConnection("taskzero-synchronizer-inmemory", _uri),
                    new UserCredentials("admin", "changeit"));
                var elasticSearchSynchronizer = new ReadModels.Elastic.SyncroniserService(BuildConnection("taskzero-synchronizer-elastic", _uri),
                    new UserCredentials("admin", "changeit"), new ReadModels.Elastic.Indexer<ReadModels.Elastic.Model.ZeroTask>(2000,
                        new ElasticClient(new Uri("http://localhost:9200")), "taskzero-tasks"));
                var commandsHandler =
                    new Handler(new EventStoreDomainRepository("domain",
                        BuildConnection("es-taskzero-domain", _uri, true)));
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
