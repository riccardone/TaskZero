using System;

namespace TaskZero.Domain
{
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(string message)
            : base(message)
        {
        }
    }
}