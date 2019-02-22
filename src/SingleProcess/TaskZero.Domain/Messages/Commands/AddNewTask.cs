using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Commands
{
    public class AddNewTask : Command
    {
        public Guid TaskId { get; }
        public string Title { get; }
        public string Description { get; }
        public DateTime? DueDate { get; }
        public Priority Priority { get; }
        public IDictionary<string, string> Metadata { get; }

        public AddNewTask(Guid taskId, string title,
            string description,
            DateTime? dueDate,
            Priority priority,
            IDictionary<string, string> metadata)
        {
            TaskId = taskId;
            Title = title;
            Description = description;
            DueDate = dueDate;
            Priority = priority;
            Metadata = metadata;
        }
    }
}
