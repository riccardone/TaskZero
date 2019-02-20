using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Events
{
    public class TaskRemoved : Event
    {
        public Guid TaskToDeleteId { get; }
        public IDictionary<string, string> Metadata { get; }

        public TaskRemoved(Guid taskToDeleteId, IDictionary<string, string> metadata)
        {
            TaskToDeleteId = taskToDeleteId;
            Metadata = metadata;
        }
    }
}
