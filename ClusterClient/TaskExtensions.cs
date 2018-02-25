using System;
using System.Threading.Tasks;

namespace ClusterClient
{
    public static class TaskExtensions
    {
        public static async Task<T> OnTimeout<T>(this Task<T> task, TimeSpan timeout, Action action)
        {
            await Task.WhenAny(task, Task.Delay(timeout));
            if (!task.IsCompleted || task.IsFaulted)
                action();

            return await task;
        }

        public static async Task<T> OnSuccess<T>(this Task<T> task, Action action)
        {
            await task;
            if (task.Status == TaskStatus.RanToCompletion)
                action();

            return task.Result;
        }

        public static async Task<T> Fallback<T>(this Task<T> task, Func<Task<T>> fallback, TimeSpan timeout)
        {
            await Task.WhenAny(task, Task.Delay(timeout));
            if (!task.IsCompleted || task.IsFaulted)
                return await fallback();

            return task.Result;
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
        {
            await Task.WhenAny(task, Task.Delay(timeout));
            if (!task.IsCompleted || task.IsFaulted)
                throw new TimeoutException();

            return task.Result;
        }
    }
}