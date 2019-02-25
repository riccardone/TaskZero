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
        private string _userName;

        public TaskPod()
        {
            _tasks = new Dictionary<Guid, Task>();
            RegisterTransition<TaskPodCreatedV1>(Apply);
            RegisterTransition<TaskAddedV1>(Apply);
            RegisterTransition<TaskRemovedV1>(Apply);
            RegisterTransition<WrongRemoveTaskRequestedV1>(Apply);
        }

        private void Apply(WrongRemoveTaskRequestedV1 obj) { }

        public TaskPod(TaskPodCreatedV1 evt) : this()
        {
            RaiseEvent(evt);
        }

        private void Apply(TaskRemovedV1 obj)
        {
            _tasks.Remove(obj.TaskToDeleteId);
        }

        private void Apply(TaskAddedV1 obj)
        {
            _tasks.Add(obj.Id, new Task(obj.Title, obj.Description, obj.DueDate, obj.Priority));
        }

        private void Apply(TaskPodCreatedV1 obj)
        {
            _correlationId = obj.Metadata["$correlationId"];
            _userName = obj.Metadata["username"];
        }

        public void AddTask(AddNewTask cmd)
        {
            CheckCommonPreconditions(cmd);
            Ensure.NotNullOrEmpty(cmd.Metadata["$correlationId"], "$correlationId");
            if (!cmd.Metadata["username"].Equals(_userName))
                return;
            if (!cmd.Metadata.ContainsKey("applies"))
                cmd.Metadata["applies"] = DateTime.UtcNow.ToString("o");
            RaiseEvent(new TaskAddedV1(cmd.TaskId, cmd.Title, cmd.Description, cmd.DueDate, cmd.Priority, cmd.Metadata));
        }

        public void RemoveTask(RemoveTask cmd)
        {
            CheckCommonPreconditions(cmd);
            Ensure.NotNullOrEmpty(cmd.Metadata["$correlationId"], "$correlationId");
            if (!cmd.Metadata["username"].Equals(_userName))
                return;
            if (!_tasks.ContainsKey(cmd.Id))
            {
                RaiseEvent(new WrongRemoveTaskRequestedV1(cmd.Id, cmd.Metadata));
                return;
            }
            RaiseEvent(new TaskRemovedV1(cmd.Id, cmd.Metadata));
        }

        public static TaskPod Create(CreateTaskPod cmd)
        {
            CheckCommonPreconditions(cmd);
            cmd.Metadata.Add("$correlationId", cmd.Id);
            cmd.Metadata.Add("applies", cmd.CreatedOn.ToString("o"));
            return new TaskPod(new TaskPodCreatedV1(cmd.Metadata));
        }

        private static void CheckCommonPreconditions(Command cmd)
        {
            Ensure.NotNull(cmd, nameof(cmd));
            Ensure.NotNull(cmd.Metadata, nameof(cmd.Metadata));
            Ensure.NotNullOrEmpty(cmd.Metadata["username"], "username");
        }
    }
}
