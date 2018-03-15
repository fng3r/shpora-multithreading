using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : AdvancedClusterClient
    {
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override Func<Queue<WebRequest>, TimeSpan, Task<string>, Task<string>> Fallback 
            => async (queue, timeout, _) => await SendNextRequestAsync(queue, timeout);

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
