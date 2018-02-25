using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ConcurrentClusterClient : ClusterClientBase
    {
        public ConcurrentClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses.Select(async addr =>
            {
                var request = CreateRequest($"{addr}?query={query}", timeout);

                return await ProcessRequestAsync(request).WithTimeout(timeout);
            });

            return await await Task.WhenAny(tasks);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ConcurrentClusterClient));
    }
}
