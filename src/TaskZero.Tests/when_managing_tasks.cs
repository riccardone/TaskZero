using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TaskZero.Domain.Aggregates;
using TaskZero.Domain.Messages.Commands;
using TaskZero.Domain.Messages.Events;

[TestFixture]
public class when_managing_tasks
{
    private string _usernameForTest = "myusernamefortest";

    [Test]
    public void given_valid_create_command_I_get_pod_created_event()
    {
        var userId = Guid.NewGuid();
        var pod = TaskPod.Create(BuildTestCreateCommand(userId.ToString(), _usernameForTest));
        Assert.IsTrue(pod.UncommitedEvents().Single().Metadata["$correlationId"].Equals(userId.ToString()));
    }

    [Test]
    public void given_a_taskPod_I_can_add_tasks()
    {
        var userId = Guid.NewGuid();
        var pod = TaskPod.Create(BuildTestCreateCommand(userId.ToString(), _usernameForTest));
        var taskId1 = Guid.NewGuid();
        var taskId2 = Guid.NewGuid();
        pod.AddTask(BuildTestAddNewTaskCommand(taskId1, pod.AggregateId, _usernameForTest));
        pod.AddTask(BuildTestAddNewTaskCommand(taskId2, pod.AggregateId, _usernameForTest));
        Assert.IsTrue(pod.UncommitedEvents().First().Metadata["$correlationId"].Equals(userId.ToString()));
        Assert.IsTrue(pod.UncommitedEvents().ToArray()[1].Metadata["$correlationId"].Equals(userId.ToString()));
        Assert.IsTrue(pod.UncommitedEvents().ToArray()[2].Metadata["$correlationId"].Equals(userId.ToString()));
        Assert.IsTrue(((TaskAdded)pod.UncommitedEvents().ToArray()[1]).Id.Equals(taskId1));
        Assert.IsTrue(((TaskAdded)pod.UncommitedEvents().ToArray()[2]).Id.Equals(taskId2));
    }

    [Test]
    public void given_a_taskPod_I_can_add_tasks_only_for_my_userName()
    {
        var userId = Guid.NewGuid();
        var pod = TaskPod.Create(BuildTestCreateCommand(userId.ToString(), _usernameForTest));
        var taskId1 = Guid.NewGuid();
        var taskId2 = Guid.NewGuid();
        pod.AddTask(BuildTestAddNewTaskCommand(taskId1, pod.AggregateId, _usernameForTest));
        pod.AddTask(BuildTestAddNewTaskCommand(taskId2, pod.AggregateId, "anotheruser"));
        Assert.IsTrue(pod.UncommitedEvents().Count() == 2);
        Assert.IsTrue(pod.UncommitedEvents().First().Metadata["$correlationId"].Equals(userId.ToString()));
        Assert.IsTrue(pod.UncommitedEvents().ToArray()[1].Metadata["$correlationId"].Equals(userId.ToString()));
        Assert.IsTrue(((TaskAdded)pod.UncommitedEvents().ToArray()[1]).Id.Equals(taskId1));
    }

    [Test]
    public void given_a_taskPod_I_can_remove_tasks()
    {
        var userId = Guid.NewGuid();
        var taskId1 = Guid.NewGuid();
        var taskId2 = Guid.NewGuid();
        var pod = BuildTaskPodWith2Tasks(userId.ToString(), _usernameForTest, taskId1, taskId2);
        pod.RemoveTask(BuildRemoveTask(taskId1, pod.AggregateId, _usernameForTest));
        Assert.IsTrue(pod.UncommitedEvents().Count() == 4);
        Assert.IsTrue(((TaskRemoved)pod.UncommitedEvents().ToArray()[3]).TaskToDeleteId.Equals(taskId1));
    }

    private TaskPod BuildTaskPodWith2Tasks(string userId, string username, Guid taskId1, Guid taskId2)
    {
        var pod = TaskPod.Create(BuildTestCreateCommand(userId, username));
        pod.AddTask(BuildTestAddNewTaskCommand(taskId1, pod.AggregateId, _usernameForTest));
        pod.AddTask(BuildTestAddNewTaskCommand(taskId2, pod.AggregateId, _usernameForTest));
        return pod;
    }

    private CreateTaskPod BuildTestCreateCommand(string userId, string userName)
    {
        const string sourceName = "test";
        return new CreateTaskPod(userId, DateTime.Now,
            new Dictionary<string, string> {{"source", sourceName}, {"username", userName}});
    }

    private AddNewTask BuildTestAddNewTaskCommand(Guid taskId, string correlationId, string userName)
    {
        return new AddNewTask(taskId, "test", "test", DateTime.Now.AddDays(1), Priority.High,
            new Dictionary<string, string>
            {
                {"$correlationId", correlationId},
                {"source", "test"},
                {"username", userName}
            });
    }

    private RemoveTask BuildRemoveTask(Guid taskId, string correlationId, string userName)
    {
        return new RemoveTask(taskId, new Dictionary<string, string>
        {
            {"$correlationId", correlationId},
            {"source", "test"},
            {"username", userName}
        });
    }
}