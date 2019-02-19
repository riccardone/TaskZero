using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Events
{
    public class TaskPodCreated : Event
    {
        public IDictionary<string, string> Metadata { get; }

        public TaskPodCreated(IDictionary<string, string> metadata)
        {
            Metadata = metadata;
        }
    }
}
