using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MoreLinq;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var requests = ReplicaAddresses
                .Permutations().First()
                .Select(addr => CreateRequest($"{addr}?query={query}", timeout)).ToArray();
            var requestQueue = new Queue<WebRequest>(requests);

            var singleRequestTimeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / requests.Length);
            var tasks = new ConcurrentBag<Task<string>>();
            var evnt = new AutoResetEvent(false);

            tasks.Add(SendNextRequest(requestQueue, singleRequestTimeout, tasks, evnt));

            Func<Task<string>> func = async () =>
            {
                if (evnt.WaitOne(timeout))
                    return await tasks.First(t => t.Status == TaskStatus.RanToCompletion);

                throw new TimeoutException();
            };
            return await func();
        }

        private async Task<string> SendNextRequest(Queue<WebRequest> requestQueue, TimeSpan timeout, ConcurrentBag<Task<string>> tasks, AutoResetEvent evnt)
        {
            if (requestQueue.Count == 0)
                throw new TimeoutException();

            return await ProcessRequestAsync(requestQueue.Dequeue())
                .OnTimeout(timeout, () => tasks.Add(SendNextRequest(requestQueue, timeout, tasks, evnt)))
                .OnSuccess(() => evnt.Set());
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
