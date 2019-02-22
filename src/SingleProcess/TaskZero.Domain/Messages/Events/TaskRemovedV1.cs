using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Events
{
    public class TaskRemovedV1 : Event
    {
        public Guid TaskToDeleteId { get; }
        public IDictionary<string, string> Metadata { get; }

        public TaskRemovedV1(Guid taskToDeleteId, IDictionary<string, string> metadata)
        {
            TaskToDeleteId = taskToDeleteId;
            Metadata = metadata;
        }
    }
}
