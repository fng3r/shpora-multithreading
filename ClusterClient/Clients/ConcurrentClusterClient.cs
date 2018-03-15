using System;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using log4net;
using TaskExtensions = ClusterClient.Extensions.TaskExtensions;

namespace ClusterClient.Clients
{
    public class ConcurrentClusterClient : ClusterClientBase
    {
        public ConcurrentClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        protected override async Task<string> ProcessRequestAsyncInternal(string query, TimeSpan timeout)
        {
            // в случаае, если одна реплика не работает, то ъждать ответа от остальных
            var tasks = ReplicaAddresses.Select(async addr =>
            {
                var request = CreateRequest($"{addr}?query={query}", timeout);

                return await ProcessRequestAsync(request).WithTimeout(timeout);
            });

            return await await TaskExtensions.WhenAnySucceded(tasks);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ConcurrentClusterClient));
    }
}
