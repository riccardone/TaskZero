﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using TaskZero.ReadModels.InMemory;
using TaskZero.ReadModels.InMemory.Model;

namespace TaskZero.Console
{
    public class Worker
    {
        private readonly SynchroniserService _synchroniserService;
        private readonly IMessageSender _sender;
        private readonly string _sourceName;
        private string _userName;
        private string _correlationId;

        public Worker(string sourceName, SynchroniserService synchroniserService, IMessageSender sender)
        {
            _sourceName = sourceName;
            _synchroniserService = synchroniserService;
            _sender = sender;
            InitReadModel();
        }

        public void Run()
        {
            System.Console.WriteLine("Write your name and press enter please...");
            _userName = System.Console.ReadLine();
            _correlationId = Deterministic.Create(Deterministic.Namespaces.Commands, Encoding.ASCII.GetBytes(_userName))
                .ToString();
            _sender.Post("api/v1/input",
                JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    {"correlationid", _correlationId},
                    {"applies", DateTime.Now.ToString("o")},
                    {"source", _sourceName},
                    {"username", _userName}
                }));
            RunToDoView();
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

        private void RunToDoView()
        {
            do
            {
                System.Console.Clear();
                // Watch out for the Eventual Consistency in case there is not enough time for the synchroniser to keep the inmemory cache up-to-date
                // This is a console app and I can't update the UI with async code :)
                Thread.Sleep(200);
                foreach (var g in _synchroniserService.Cache.GroupBy(a => a.Key))
                {
                    System.Console.WriteLine(g.Key.Equals(_userName)
                        ? $"----------{g.Key}'s-TODO-------------------"
                        : $"----------{g.Key}'s-TODO-(read-only)--------");
                    foreach (var todo in g)
                        foreach (var task in todo.Value)
                            System.Console.WriteLine($"Id: {task.Key}, {task.Value}");
                }
                System.Console.WriteLine("------------------------------------------");
                System.Console.WriteLine($"You are managing {_userName}'s TODO list");
                System.Console.WriteLine("Press A to add a new task");
                System.Console.WriteLine("Press D to remove a task");
                System.Console.WriteLine("Press C to change pod");
                System.Console.WriteLine("Press R to delete pod");

                var key = System.Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.A:
                        _sender.Post("AddNewTask", BuildAddNewTaskCommand());
                        break;
                    case ConsoleKey.D:
                        var remove = BuildRemoveTask();
                        if (remove != null)
                            _sender.Post("RemoveTask", remove);
                        break;
                    case ConsoleKey.C:
                        System.Console.Clear();
                        Run();
                        break;
                    case ConsoleKey.R:
                        var delete = BuildDeleteTaskPod();
                        if (delete != null)
                        {
                            _sender.Post("DeleteTaskPod", delete);
                            System.Console.Clear();
                            Run();
                        }
                        break;
                }
            } while (true);
        }

        private Dictionary<string, string> BuildDeleteTaskPod()
        {
            System.Console.WriteLine($"Are you sure that you want permanently delete {_userName}'s TODO and its content?");
            System.Console.WriteLine($"You will not be able to reuse the same delete {_userName} name for another TaskPod (Y to confirm, N to cancel)");
            var key = System.Console.ReadKey();
            if (key.Key == ConsoleKey.Y)
            {
                return new Dictionary<string, string>
                {
                    {"$correlationId", _correlationId},
                    {"source", _sourceName},
                    {"username", _userName}
                };
            }
            return null;
        }

        private Dictionary<string, string> BuildRemoveTask()
        {
            System.Console.WriteLine("Task ID?");
            var idText = System.Console.ReadLine();
            if (Guid.TryParse(idText, out var idToDelete))
            {
                return new Dictionary<string, string>
                {
                    {"idtodelete", idToDelete.ToString()},
                    {"$correlationId", _correlationId},
                    {"source", _sourceName},
                    {"username", _userName}
                };
            }
            System.Console.WriteLine("Not valid id");
            Thread.Sleep(1000);
            return null;
        }

        private Dictionary<string, string> BuildAddNewTaskCommand()
        {
            System.Console.WriteLine("Title? (default: test)");
            var title = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
                title = "test";
            System.Console.WriteLine("Description? (default: test)");
            var description = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(description))
                description = "test";
            System.Console.WriteLine("Due Date? yyyy-mm-dd (Default: null)");
            DateTime? dueDate = null;
            if (DateTime.TryParse(System.Console.ReadLine(), out var dueDateVal))
                dueDate = dueDateVal;
            System.Console.WriteLine("Priority? NotSet=0, Low=1, Normal=2, High=3, Urgent=4 (default NotSet=0)");
            Enum.TryParse(System.Console.ReadLine(), out Priority priority);
            return new Dictionary<string, string>
            {
                {"id", Guid.NewGuid().ToString()},
                {"title", title},
                {"description", description},
                {"duedate", dueDate?.ToString("o")},
                {"priority", priority.ToString()},
                {"$correlationId", _correlationId},
                {"source", _sourceName},
                {"username", _userName}
            };
        }
    }
}