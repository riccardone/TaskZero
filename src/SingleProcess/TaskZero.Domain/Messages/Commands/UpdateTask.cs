using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Commands
{
    public class UpdateTask : Command
    {
        public Guid Id { get; }
        public string Title { get; }
        public string Description { get; }
        public DateTime? DueDate { get; }
        public Priority Priority { get; }
        public IDictionary<string, string> Metadata { get; }

        public UpdateTask(Guid id, string title,
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
