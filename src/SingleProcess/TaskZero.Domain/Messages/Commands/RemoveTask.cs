using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Commands
{
    public class RemoveTask : Command
    {
        public Guid Id { get; }
        public IDictionary<string, string> Metadata { get; }

        public RemoveTask(Guid id, IDictionary<string, string> metadata)
        {
            Id = id;
            Metadata = metadata;
        }
    }
}
