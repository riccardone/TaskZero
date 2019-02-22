using System;

namespace TaskZero.ReadModels.InMemory.Model
{
    public class ZeroTask
    {
        public string Title { get; }
        public string Description { get; }
        public DateTime? DueDate { get; }
        public Priority Priority { get; }
        public string Source { get; }

        public ZeroTask(string title, string description, DateTime? dueDate, Priority priority, string source)
        {
            Title = title;
            Description = description;
            DueDate = dueDate;
            Priority = priority;
            Source = source;
        }

        public override string ToString()
        {
            return
                $"Priority: {Priority.ToString()}, Title: {Title}, Description {(Description.Length > 20 ? Description.Substring(0, 20) : Description)}, DueDate: {(DueDate.HasValue ? DueDate.Value.ToShortDateString() : "none")}, Source: {Source}";
        }
    }

    public enum Priority
    {
        NotSet = 0,
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }
}
