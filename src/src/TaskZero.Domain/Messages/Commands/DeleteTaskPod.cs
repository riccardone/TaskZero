using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Commands
{
    public class DeleteTaskPod : Command
    {
        public string Id { get; }
        public IDictionary<string, string> Metadata { get; }

        public DeleteTaskPod(string id, IDictionary<string, string> metadata)
        {
            Id = id;
            Metadata = metadata;
        }
    }
}
