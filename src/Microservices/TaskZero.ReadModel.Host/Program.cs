using System;
using System.Linq;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using NLog;
using NLog.Config;
using NLog.Targets;
using TaskZero.ReadModels.InMemory;

namespace TaskZero.ReadModel.Host
{
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static Uri _uri;

        static void Main(string[] args)
        {
            ConfigureLogging();

            var es = "localhost:1113";
            if (args.Length > 0)
                es = args[0];
            _uri = new Uri($"tcp://{es}");

            
            var inputConnection = BuildConnection("es-taskzero-input", _uri);
            inputConnection.ConnectAsync().Wait();

            var synchroniserService =
                new SynchroniserService(inputConnection, new UserCredentials("admin", "changeit"));
            synchroniserService.Start().Wait();

            do
            {
                Console.WriteLine("Press R to refresh the view");
                Console.WriteLine("Press CTRL+C to exit");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.R)
                {
                    RefreshView(synchroniserService);
                }
            } while (true);
        }

        private static void RefreshView(SynchroniserService synchroniserService)
        {
            foreach (var g in synchroniserService.Cache.GroupBy(a => a.Key))
            {
                Log.Info($"----------{g.Key}'s-TODO-------------------");
                foreach (var todo in g)
                foreach (var task in todo.Value)
                    Log.Info($"Id: {task.Key}, {task.Value}");
            }
            Log.Info($"------------------------------------");
        }

        private static IEventStoreConnection BuildConnection(string name, Uri uri)
        {
            return EventStoreConnection.Create(ConnectionSettings.Create().KeepRetrying().KeepReconnecting(), uri,
                name);
        }

        private static void ConfigureLogging()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget("target1")
            {
                //Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception}"
            };
            config.AddTarget(consoleTarget);
            config.AddRuleForAllLevels(consoleTarget);
            LogManager.Configuration = config;
        }
    }
}
