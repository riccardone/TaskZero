using System;
using System.Collections.Generic;
using TaskZero.Domain.Messages.Commands;
using TaskZero.Domain.Messages.Events;

namespace TaskZero.Domain.Aggregates
{
    public class TaskPod : AggregateBase
    {
        public override string AggregateId => _correlationId;
        private string _correlationId;
        private readonly IDictionary<Guid, Task> _tasks;

        public TaskPod()
        {
            _tasks = new Dictionary<Guid, Task>();
            RegisterTransition<TaskPodCreated>(Apply);
            RegisterTransition<TaskAdded>(Apply);
            RegisterTransition<TaskDeleted>(Apply);
        }

        public TaskPod(TaskPodCreated evt) : this()
        {
            RaiseEvent(evt);
        }

        private void Apply(TaskDeleted obj)
        {
            _tasks.Remove(obj.TaskToDeleteId);
        }

        private void Apply(TaskAdded obj)
        {
            _tasks.Add(obj.Id, new Task(obj.Title, obj.Description, obj.DueDate, obj.Priority));
        }

        private void Apply(TaskPodCreated obj)
        {
            _correlationId = obj.Metadata["$correlationId"];
        }

        public List<Event> AddTask(AddNewTask cmd)
        {
            // TODO write your validation logic
            var evt = new TaskAdded(cmd.TaskId, cmd.Title, cmd.Description, cmd.DueDate, cmd.Priority, cmd.Metadata);
            RaiseEvent(evt);
            return new List<Event> {evt};
        }

        public List<Event> RemoveTask(RemoveTask cmd)
        {
            // TODO write your validation logic
            var evt = new TaskDeleted(cmd.Id, cmd.Metadata);
            if (!_tasks.ContainsKey(cmd.Id))
                return new List<Event>();
            RaiseEvent(evt);
            return new List<Event> { evt };
        }

        public static TaskPod Create(CreateTaskPod cmd)
        {
            // TODO write your validation logic
            cmd.Metadata.Add("$correlationId", cmd.Id);
            cmd.Metadata.Add("applies", cmd.CreatedOn.ToString("o"));
            return new TaskPod(new TaskPodCreated(cmd.Metadata));
        }
    }
}
