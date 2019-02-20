using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Events
{
    public class TaskPodCreatedV1 : Event
    {
        public IDictionary<string, string> Metadata { get; }

        public TaskPodCreatedV1(IDictionary<string, string> metadata)
        {
            Metadata = metadata;
        }
    }
}
