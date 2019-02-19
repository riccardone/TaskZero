using System;
using System.Collections.Generic;
using TaskZero.Domain.Messages.Commands;

namespace TaskZero.Domain.Messages.Events
{
    public class TaskAdded : Event
    {
        public Guid Id { get; }
        public string Title { get; }
        public string Description { get; }
        public DateTime? DueDate { get; }
        public Priority Priority { get; }
        public IDictionary<string, string> Metadata { get; }

        public TaskAdded(Guid id, string title,
            string description,
            DateTime? dueDate,
            Priority priority,
            IDictionary<string, string> metadata)
        {
            Id = id;
            Title = title;
            Description = description;
            DueDate = dueDate;
            Priority = priority;
            Metadata = metadata;
        }
    }
}
