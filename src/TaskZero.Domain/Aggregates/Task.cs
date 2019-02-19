using System;
using TaskZero.Domain.Messages.Commands;

namespace TaskZero.Domain.Aggregates
{
    public class Task
    {
        public string Title { get; }
        public string Description { get; }
        public DateTime? DueDate { get; }
        public Priority Priority { get; }

        public Task(string title, string description, DateTime? dueDate, Priority priority)
        {
            Title = title;
            Description = description;
            DueDate = dueDate;
            Priority = priority;
        }

        public override string ToString()
        {
            return
                $"Priority: {Priority.ToString()}, Title: {Title}, Description {(Description.Length > 20 ? Description.Substring(0, 20) : Description)}, DueDate: {(DueDate.HasValue ? DueDate.Value.ToShortDateString() : string.Empty)}";
        }
    }
}
