using System.Collections.Generic;

namespace TaskZero.Domain.Messages
{
    public interface Message 
    {
        IDictionary<string, string> Metadata { get; }
    }
}
