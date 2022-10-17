using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace RateLimiterCore;

public class RateLimiter<T> : IRateLimiter<T>, IDisposable
{
    private readonly IRateLimiterPreferences _rateLimiterPreferences;
    //private readonly Queue<DateTime> _launchedTaskTimes;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<Task> _launchedTasks;
    private const int WaitTasksCompletedSeconds = 30;
    private readonly object _queueLocker = new();
    private bool _disposed;
    private const int MillisecondsPerSecond = 1000;

    public RateLimiter(IRateLimiterPreferences rateLimiterPreferences)
    {
        _rateLimiterPreferences = rateLimiterPreferences;
        _semaphore = new SemaphoreSlim(rateLimiterPreferences.RequestsPerInterval);
        _launchedTasks = new ConcurrentQueue<Task>();
    }
    public async Task<Result<T>> Invoke(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action), "Action parameter should not be null");
        }
        
        if (!TryTakeSemaphore(cancellationToken))
        {
            return Result<T>.Fail();
        }
        
        Task.Run(async () =>
        {
            await Task.Delay(_rateLimiterPreferences.IntervalSeconds * MillisecondsPerSecond);
            _semaphore.Release();
        }, cancellationToken);
        
        Task<T>? newTask = null;
        cancellationToken.ThrowIfCancellationRequested();

        if (_rateLimiterPreferences.DoNotLaunchTasks)
        {
            return Result<T>.Fail();
        }

        if (_rateLimiterPreferences.SerialMode)
        {
            newTask = LaunchBySerialWay(action, cancellationToken);
        }
        else
        {
            newTask = action();
            UpdateTaskQueue(newTask);
        }

        var value = await newTask;
        _launchedTasks.TryDequeue(out _);
        return Result<T>.Success(value);
    }

    private Task<T> LaunchBySerialWay(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        Task<T> newTask;
        lock (_queueLocker)
        {
            Task.WaitAll(_launchedTasks.ToArray(), cancellationToken);
            newTask = action();
            UpdateTaskQueue(newTask);
        }

        return newTask;
    }

    public void Dispose()
    {
        TryWaitAllTasks();
        _semaphore.Dispose();
    }
    
    protected virtual void TryWaitAllTasks()
    {
        if (_disposed)
        {
            return;
        }
        
        lock (_queueLocker)
        {
            var waitTasksCompletedMilliseconds = WaitTasksCompletedSeconds * MillisecondsPerSecond;
            Task.WaitAll(_launchedTasks.ToArray(), waitTasksCompletedMilliseconds);
            _disposed = true;
        }
    }
    
    private void UpdateTaskQueue(Task<T> newTask)
    {
        if (!_disposed)
        {
            _launchedTasks.Enqueue(newTask);
        }
    }

    private bool TryTakeSemaphore(CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            return false;
        }

        return _semaphore.Wait(TimeSpan.Zero, cancellationToken);
    }
}