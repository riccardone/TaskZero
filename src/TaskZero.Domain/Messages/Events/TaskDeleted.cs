using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Events
{
    public class TaskDeleted : Event
    {
        public Guid TaskToDeleteId { get; }
        public IDictionary<string, string> Metadata { get; }

        public TaskDeleted(Guid taskToDeleteId, IDictionary<string, string> metadata)
        {
            TaskToDeleteId = taskToDeleteId;
            Metadata = metadata;
        }
    }
}
