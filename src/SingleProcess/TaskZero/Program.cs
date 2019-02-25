using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Nest;
using TaskZero.Adapter;
using TaskZero.ReadModels.Elastic;
using TaskZero.Repository.EventStore;
using ConnectionSettings = EventStore.ClientAPI.ConnectionSettings;
using SynchroniserService = TaskZero.ReadModels.InMemory.SynchroniserService;

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
                var synchroniserService = new SynchroniserService(BuildConnection("taskzero-syncroniser-inmemory", _uri),
                    new UserCredentials("admin", "changeit"));
                var domainConnection = BuildConnection("es-taskzero-domain", _uri);
                domainConnection.ConnectAsync().Wait();
                var domainRepository = new EventStoreDomainRepository("domain", domainConnection);
                var handler = new Handler(domainRepository);
                var worker = new Worker("TaskZero.Console", synchroniserService,
                    new ReadModels.Elastic.SynchroniserService(BuildConnection("taskzero-syncroniser-elastic", _uri),
                        new UserCredentials("admin", "changeit"), new Indexer<ReadModels.Elastic.Model.ZeroTask>(2000,
                            new ElasticClient(new Uri("http://localhost:9200")), "taskzero-tasks")), handler);
                worker.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static IEventStoreConnection BuildConnection(string name, Uri uri)
        {
            return EventStoreConnection.Create(ConnectionSettings.Create().KeepRetrying().KeepReconnecting(), uri, name);
        }
    }
}
