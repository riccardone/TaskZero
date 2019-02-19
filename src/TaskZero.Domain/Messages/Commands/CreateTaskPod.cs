using System;
using System.Collections.Generic;

namespace TaskZero.Domain.Messages.Commands
{
    public class CreateTaskPod : Command
    {
        public string Id { get; }
        public DateTime CreatedOn { get; }
        public IDictionary<string, string> Metadata { get; }

        public CreateTaskPod(string id, DateTime createdOn, IDictionary<string, string> metadata)
        {
            Id = id;
            CreatedOn = createdOn;
            Metadata = metadata;
        }
    }
}
