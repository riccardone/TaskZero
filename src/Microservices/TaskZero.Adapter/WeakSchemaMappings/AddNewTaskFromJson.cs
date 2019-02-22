using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TaskZero.Domain.Messages.Commands;

namespace TaskZero.Adapter.WeakSchemaMappings
{
    public class AddNewTaskFromJson : Command
    {
        public Guid TaskId { get; }
        public string Title { get; }
        public string Description { get; }
        public DateTime? DueDate { get; }
        public Priority Priority { get; }
        public IDictionary<string, string> Metadata { get; }

        public AddNewTaskFromJson(string bodyAsJson, string metadataAsJson)
        {
            var body = JsonConvert.DeserializeObject<dynamic>(bodyAsJson);
            var metadata = JsonConvert.DeserializeObject<IDictionary<string, string>>(metadataAsJson);
            
            TaskId = body.taskId.Value;
            Title = body.title.Value;
            Description = body.description.Value;
            DueDate = body.dueDate.Value;
            Priority = body.priority.Value;
            Metadata = metadata;
        }

        
    }
}
