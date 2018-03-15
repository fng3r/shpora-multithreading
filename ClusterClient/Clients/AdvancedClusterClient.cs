using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using ClusterClient.Utils;

namespace ClusterClient.Clients
{
    public abstract class AdvancedClusterClient : ClusterClientBase
    {
        protected readonly TtlStringHashSet greylist = new TtlStringHashSet();
        protected readonly TimeSpan greylistStayDuration = TimeSpan.FromSeconds(30);
        protected readonly ServerStatistics replicaStatistics;

        protected AdvancedClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
            replicaStatistics = new ServerStatistics();
        }

        protected override async Task<string> ProcessRequestAsyncInternal(string query, TimeSpan timeout)
        {
            if (greylist.Count() == ReplicaAddresses.Length)
                greylist.Clear();

            var availableReplicas = ReplicaAddresses.Except(greylist).ToArray();

            var singleRequestTimeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds /availableReplicas.Length);
            var requests = availableReplicas
                .OrderBy(replicaStatistics.GetAverageResponseTime)
                .Select(addr => CreateRequest($"{addr}?query={query}", timeout)).ToArray();
            var requestQueue = new Queue<WebRequest>(requests);

            return await SendNextRequestAsync(requestQueue, singleRequestTimeout);
        }

        public abstract Func<Queue<WebRequest>, TimeSpan, Task<string>, Task<string>> Fallback { get; }

        protected async Task<string> SendNextRequestAsync(Queue<WebRequest> requestQueue, TimeSpan timeout)
        {
            if (requestQueue.Count == 0)
                throw new TimeoutException();

            var request = requestQueue.Dequeue();
            var replicaAddress = request.RequestUri.GetLeftPart(UriPartial.Path);
            var sw = Stopwatch.StartNew();
            var task = ProcessRequestAsync(request);

            return await task
                .Then(() => replicaStatistics.AddData(replicaAddress, sw.ElapsedMilliseconds), timeout)
                .Fallback(() =>
                {
                    greylist.Add(replicaAddress, DateTime.Now.Add(greylistStayDuration));

                    return Fallback(requestQueue, timeout, task);
                }, timeout);
        }
    }
}
