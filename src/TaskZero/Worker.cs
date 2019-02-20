using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TaskZero.Adapter;
using TaskZero.Domain.Messages.Commands;
using TaskZero.ReadModels.InMemory;

namespace TaskZero
{
    public class Worker
    {
        private readonly SynchroniserService _synchroniserService;
        private readonly Handler _handler;
        private readonly string _sourceName;
        private string _userName;
        private string _correlationId;

        public Worker(string sourceName, SynchroniserService synchroniserService, Handler handler)
        {
            _sourceName = sourceName;
            _synchroniserService = synchroniserService;
            _handler = handler;
            InitReadModel();
        }

        public void Run()
        {
            Console.WriteLine("Write your name and press enter please...");
            _userName = Console.ReadLine();
            _correlationId = Deterministic.Create(Deterministic.Namespaces.Commands, Encoding.ASCII.GetBytes(_userName))
                .ToString();
            _handler.Handle(new CreateTaskPod(_correlationId, DateTime.Now,
                new Dictionary<string, string> {{"source", _sourceName}, {"username", _userName } }));
            RunToDoView(_handler);
        }

        private void InitReadModel()
        {
            _synchroniserService.LiveSynchStarted += SyncroniserService_LiveSynchStarted;
            _synchroniserService.Start();
        }

        private static void SyncroniserService_LiveSynchStarted(object sender, EventArgs e)
        {
            // Console.WriteLine("Cache ready");
        }

        private void RunToDoView(Handler handler)
        {
            do
            {
                Console.Clear();
                // Watch out for the Eventual Consistency in case there is not enough time for the synchroniser to keep the inmemory cache up-to-date
                // This is a console app and I can't update the UI with async code :)
                Thread.Sleep(200);
                foreach (var g in _synchroniserService.Cache.GroupBy(a => a.Key))
                {
                    Console.WriteLine(g.Key.Equals(_userName)
                        ? $"----------{g.Key}'s-TODO-------------------"
                        : $"----------{g.Key}'s-TODO-(read-only)--------");
                    foreach (var todo in g)
                        foreach (var task in todo.Value)
                            Console.WriteLine($"Id: {task.Key}, {task.Value}");
                }
                Console.WriteLine("------------------------------------------");
                Console.WriteLine($"You are managing {_userName}'s TODO list");
                Console.WriteLine("Press A to add a new task");
                Console.WriteLine("Press D to remove a task");
                Console.WriteLine("Press C to change pod");

                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.A:
                        handler.Handle(BuildAddNewTaskCommand());
                        break;
                    case ConsoleKey.D:
                        handler.Handle(BuildRemoveTask());
                        break;
                    case ConsoleKey.C:
                        Run();
                        break;
                }
            } while (true);
        }

        private RemoveTask BuildRemoveTask()
        {
            Console.WriteLine("Task ID?");
            var idToDelete = Console.ReadLine();
            return new RemoveTask(Guid.Parse(idToDelete),
                new Dictionary<string, string>
                {
                    {"$correlationId", _correlationId},
                    {"source", _sourceName},
                    {"username", _userName}
                });
        }

        private AddNewTask BuildAddNewTaskCommand()
        {
            Console.WriteLine("Title? (default: test)");
            var title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
                title = "test";
            Console.WriteLine("Description? (default: test)");
            var description = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(description))
                description = "test";
            Console.WriteLine("Due Date? yyyy-mm-dd (Default: null)");
            DateTime? dueDate = null;
            if (DateTime.TryParse(Console.ReadLine(), out var dueDateVal))
                dueDate = dueDateVal;
            Console.WriteLine("Priority? NotSet=0, Low=1, Normal=2, High=3, Urgent=4 (default NotSet=0)");
            Enum.TryParse(Console.ReadLine(), out Priority priority);
            return new AddNewTask(Guid.NewGuid(), title, description, dueDate, priority,
                new Dictionary<string, string>
                {
                    {"$correlationId", _correlationId},
                    {"source", _sourceName},
                    {"username", _userName}
                });
        }
    }
}
