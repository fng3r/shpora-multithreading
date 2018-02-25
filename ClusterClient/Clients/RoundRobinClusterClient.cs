using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;
using MoreLinq;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var singleRequestTimeout = TimeSpan.FromMilliseconds(timeout.TotalMilliseconds / ReplicaAddresses.Length);
            var requests = ReplicaAddresses
                .Permutations().First()
                .Select(addr => CreateRequest($"{addr}?query={query}", singleRequestTimeout)).ToArray();
            var queue = new Queue<WebRequest>(requests);
            
            return await SendNextRequest(queue, singleRequestTimeout);
        }

        private async Task<string> SendNextRequest(Queue<WebRequest> requestQueue, TimeSpan timeout)
        {
            if (requestQueue.Count == 0)
                throw new TimeoutException();

            return await ProcessRequestAsync(requestQueue.Dequeue())
                .Fallback(() => SendNextRequest(requestQueue, timeout), timeout);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}