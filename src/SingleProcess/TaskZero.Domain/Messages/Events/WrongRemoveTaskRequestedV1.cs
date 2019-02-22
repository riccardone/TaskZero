using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Events
{
    public class WrongRemoveTaskRequestedV1 : Event
    {
        public Guid TaskToDeleteId { get; }
        public IDictionary<string, string> Metadata { get; }

        public WrongRemoveTaskRequestedV1(Guid taskToDeleteId, IDictionary<string, string> metadata)
        {
            TaskToDeleteId = taskToDeleteId;
            Metadata = metadata;
        }
    }
}
