using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClusterClient.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<Task<T>> WhenAnySucceded<T>(IEnumerable<Task<T>> tasks)
        {
            return await WhenAnySucceded(tasks.ToArray());
        }

        public static async Task<Task<T>> WhenAnySucceded<T>(params Task<T>[] tasks)
        {
            if (tasks.Length == 0)
                //throw new ArgumentException("empty sequence");
                return null;

            var task = await Task.WhenAny(tasks);
            if (task.Status == TaskStatus.RanToCompletion)
                return task;

            return await WhenAnySucceded(tasks.Where(t => t != task).ToArray());
        }

        public static async Task<T> OnTimeout<T>(this Task<T> task, TimeSpan timeout, Action action)
        {
            await Task.WhenAny(task, Task.Delay(timeout));
            if (!task.IsCompleted || task.IsFaulted)
                action();

            return await task;
        }

        public static async Task<T> Then<T>(this Task<T> task, Action action, TimeSpan timeout)
        {
            await Task.WhenAny(task, Task.Delay(timeout));
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