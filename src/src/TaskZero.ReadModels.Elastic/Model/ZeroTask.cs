using System;
using Nest;

namespace TaskZero.ReadModels.Elastic.Model
{
    [ElasticsearchType(IdProperty = "Id")]
    public class ZeroTask
    {
        public string Id { get; }
        public string PodName { get; }
        public string Title { get; }
        public string Description { get; }
        public DateTime? DueDate { get; }
        public string Priority { get; }
        public string Source { get; }
        public string CorrelationId { get; }
        public DateTime CreatedOn { get; }

        public ZeroTask(string id, string podName, string title, string description, DateTime? dueDate, string priority, string source, string correlationId, DateTime createdOn)
        {
            Id = id;
            PodName = podName;
            Title = title;
            Description = description;
            DueDate = dueDate;
            Priority = priority;
            Source = source;
            CorrelationId = correlationId;
            CreatedOn = createdOn;
        }

        public override string ToString()
        {
            return
                $"Priority: {Priority}, Title: {Title}, Description {(Description.Length > 20 ? Description.Substring(0, 20) : Description)}, DueDate: {(DueDate.HasValue ? DueDate.Value.ToShortDateString() : "none")}, Source: {Source}";
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
