using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace URBE.Pokemon.API.Services;
public static class BackgroundTaskStore
{
    private readonly record struct TaskCapsule(Task Task, Func<Task, ValueTask>? OnFailure);
    private readonly static ConcurrentBag<TaskCapsule> _tasks = new();
    private static bool active = true;

    static BackgroundTaskStore()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) => active = false;
    }

    /// <summary>
    /// Adds a new background task to the store
    /// </summary>
    /// <param name="task">The task to add</param>
    /// <param name="onCompletion">An action to execute when the task completes, whether due to an error or not.</param>
    public static bool Add(Task task, Func<Task, ValueTask>? onCompletion = null)
    {
        if (active is false) return false;
        _tasks.Add(new TaskCapsule(task, onCompletion));
        return true;
    }

    /// <summary>
    /// Adds a new background task to the store
    /// </summary>
    /// <param name="task">The task to add</param>
    /// <param name="onCompletion">An action to execute when the task completes, whether due to an error or not.</param>
    public static bool Add(Func<Task> task, bool reschedule, Func<Task, ValueTask>? onCompletion = null)
    {
        if (active is false) return false;
        if (reschedule)
            if (onCompletion is not null)
                _tasks.Add(new TaskCapsule(task(), async t =>
                {
                    await onCompletion.Invoke(t);
                    Add(task, reschedule, onCompletion);
                }));
            else
                _tasks.Add(new TaskCapsule(task(), async t =>
                {
                    Add(task, reschedule, onCompletion);
                    await t;
                }));
        else
            _tasks.Add(new TaskCapsule(task(), onCompletion));

        return true;
    }

    /// <summary>
    /// Performs a single sweep on the store, searching for completed tasks to await
    /// </summary>
    public static async Task Sweep()
    {
        List<Exception>? exceptions = null;
        foreach (var (task, onf) in _tasks)
        {
            if (task.IsCompleted)
                try
                {
                    if (onf is null)
                        await task;
                    else
                        await onf.Invoke(task);
                }
                catch (Exception e)
                {
                    (exceptions ??= new()).Add(e);
                }
        }

        if (exceptions?.Count is > 0)
            throw new AggregateException(exceptions);
    }
}
