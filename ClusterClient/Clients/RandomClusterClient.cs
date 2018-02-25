using System;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RandomClusterClient : ClusterClientBase
    {
        private readonly Random random = new Random();

        public RandomClusterClient(string[] replicaAddresses)
            : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var uri = ReplicaAddresses[random.Next(ReplicaAddresses.Length)];
            var webRequest = CreateRequest($"{uri}?query={query}", timeout);
            
            Log.InfoFormat("Processing {0}", webRequest.RequestUri);

            return await ProcessRequestAsync(webRequest).WithTimeout(timeout);
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));
    }
}