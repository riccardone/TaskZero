using System;
using System.Linq;
using TaskZero.Domain;
using TaskZero.Domain.Aggregates;
using TaskZero.Domain.Messages.Commands;
using TaskZero.Domain.Messages.Events;

namespace TaskZero.Adapter
{
    public class Handler : IHandle<CreateTaskPod>, IHandle<AddNewTask>, IHandle<RemoveTask>
    {
        private readonly IDomainRepository _repo;

        public Handler(IDomainRepository repo)
        {
            _repo = repo;
        }

        public IAggregate Handle(CreateTaskPod command)
        {
            TaskPod taskPod;
            try
            {
                taskPod = _repo.GetById<TaskPod>(command.Id);
            }
            catch (AggregateNotFoundException)
            {
                taskPod = TaskPod.Create(command);
                _repo.SaveAsync(taskPod).Wait();
            }
            return taskPod;
        }

        public IAggregate Handle(AddNewTask command)
        {
            var taskPod = _repo.GetById<TaskPod>(command.Metadata["$correlationId"]);
            taskPod.AddTask(command);
            _repo.SaveAsync(taskPod);
            return taskPod;
        }

        public IAggregate Handle(RemoveTask command)
        {
            var taskPod = _repo.GetById<TaskPod>(command.Metadata["$correlationId"]);
            taskPod.RemoveTask(command);
            _repo.SaveAsync(taskPod);
            return taskPod;
        }
    }
}
