using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using log4net;
using MoreLinq;

namespace ClusterClient.Clients
{
    public class BatchClusterClient : ClusterClientBase
    {
        private readonly int batchSize;

        public BatchClusterClient(string[] replicaAddresses, int batchSize) : base(replicaAddresses)
        {
            this.batchSize = batchSize;
        }

        protected override async Task<string> ProcessRequestAsyncInternal(string query, TimeSpan timeout)
        {
            var batchCount = Math.Ceiling((double) ReplicaAddresses.Length / batchSize);
            var singleBatchTimeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / batchCount);
            var requests = ReplicaAddresses
                .RandomSubset(ReplicaAddresses.Length)
                .Select(addr => CreateRequest($"{addr}?query={query}", timeout))
                .Batch(batchSize);
            var requestQueue = requests.ToQueue();

            return await await SendNextBatchAsync(requestQueue, singleBatchTimeout);
        }

        protected virtual async Task<Task<string>> SendNextBatchAsync(Queue<IEnumerable<HttpWebRequest>> requestQueue, TimeSpan timeout)
        {
            if (requestQueue.Count == 0)
                throw new TimeoutException();

            var batch = requestQueue.Dequeue();
            var task = Extensions.TaskExtensions.WhenAnySucceded(batch.Select(req => ProcessRequestAsync(req)));

            return await task.Fallback(() => SendNextBatchAsync(requestQueue, timeout), timeout);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(BatchClusterClient));
    }
}
