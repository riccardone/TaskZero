using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TaskZero.Domain.Messages.Commands;

namespace TaskZero.Adapter.WeakSchemaMappings
{
    public class RemoveTaskFromJson : Command
    {
        public Guid Id { get; }
        public IDictionary<string, string> Metadata { get; }

        public RemoveTaskFromJson(string bodyAsJson, string metadataAsJson)
        {
            var body = JsonConvert.DeserializeObject<dynamic>(bodyAsJson);
            var metadata = JsonConvert.DeserializeObject<IDictionary<string, string>>(metadataAsJson);

            Id = body.id.Value;
            Metadata = metadata;
        }
    }
}
