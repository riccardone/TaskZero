using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Events
{
    public class WrongRemoveTaskRequested : Event
    {
        public Guid TaskToDeleteId { get; }
        public IDictionary<string, string> Metadata { get; }

        public WrongRemoveTaskRequested(Guid taskToDeleteId, IDictionary<string, string> metadata)
        {
            TaskToDeleteId = taskToDeleteId;
            Metadata = metadata;
        }
    }
}
