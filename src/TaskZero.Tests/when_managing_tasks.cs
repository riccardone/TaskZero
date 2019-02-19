using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TaskZero.Domain.Aggregates;
using TaskZero.Domain.Messages.Commands;

[TestFixture]
public class when_managing_tasks
{
    [Test]
    public void given_valid_create_command_I_get_pod_created_event()
    {
        var userId = Guid.NewGuid();
        var pod = TaskPod.Create(
            new CreateTaskPod(new Dictionary<string, string> {{"$correlationId", userId.ToString()}}));
        Assert.IsTrue(pod.UncommitedEvents().Single().Metadata["$correlationId"].Equals(userId.ToString()));
    }
}