﻿using System;
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
        private readonly SyncroniserService _inMemorySynchroniser;
        private readonly ReadModels.Elastic.SyncroniserService _elasticSynchroniser;
        private readonly Handler _handler;
        private readonly string _sourceName;
        private string _userName;
        private string _correlationId;

        public Worker(string sourceName, SyncroniserService inMemorySynchroniser, ReadModels.Elastic.SyncroniserService elasticSynchroniser, Handler handler)
        {
            _sourceName = sourceName;
            _inMemorySynchroniser = inMemorySynchroniser;
            _elasticSynchroniser = elasticSynchroniser;
            _handler = handler;
            InitReadModels();
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

        private void InitReadModels()
        {
            _inMemorySynchroniser.LiveSynchStarted += SyncroniserService_LiveSynchStarted;
            _inMemorySynchroniser.Start();
            if (_elasticSynchroniser != null)
                _elasticSynchroniser.Start();
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
                foreach (var g in _inMemorySynchroniser.Cache.GroupBy(a => a.Key))
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
                Console.WriteLine("Press R to delete pod");

                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.A:
                        handler.Handle(BuildAddNewTaskCommand());
                        break;
                    case ConsoleKey.D:
                        var remove = BuildRemoveTask();
                        if (remove != null)
                            handler.Handle(remove);
                        break;
                    case ConsoleKey.C:
                        Console.Clear();
                        Run();
                        break;
                    case ConsoleKey.R:
                        var delete = BuildDeleteTaskPod();
                        if (delete != null)
                        {
                            try
                            {
                                handler.Handle(delete);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"TaskPod {_userName} has been already deleted");
                            }
                            Console.Clear();
                            Run();
                        }
                        break;
                }
            } while (true);
        }

        private DeleteTaskPod BuildDeleteTaskPod()
        {
            Console.WriteLine($"Are you sure that you want permanently delete {_userName}'s TODO and its content?");
            Console.WriteLine($"You will not be able to reuse the same delete {_userName} name for another TaskPod (Y to confirm, N to cancel)");
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Y)
            {
                return new DeleteTaskPod(_correlationId, new Dictionary<string, string>
                {
                    {"$correlationId", _correlationId},
                    {"source", _sourceName},
                    {"username", _userName}
                });
            }
            return null;
        }

        private RemoveTask BuildRemoveTask()
        {
            Console.WriteLine("Task ID?");
            var idText = Console.ReadLine();
            if (Guid.TryParse(idText, out var idToDelete))
            {
                return new RemoveTask(idToDelete, new Dictionary<string, string>
                {
                    {"$correlationId", _correlationId},
                    {"source", _sourceName},
                    {"username", _userName}
                });
            }
            Console.WriteLine("Not valid id");
            Thread.Sleep(1000);
            return null;
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
            Console.WriteLine("Due Date? yyyy-mm-dd (Default: yesterday same time)");
            DateTime? dueDate = DateTime.Now.AddDays(-1);
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
