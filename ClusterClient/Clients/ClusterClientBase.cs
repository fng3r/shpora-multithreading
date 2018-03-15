using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public abstract class ClusterClientBase
    {
        protected string[] ReplicaAddresses { get; set; }

        protected ClusterClientBase(string[] replicaAddresses)
        {
            ReplicaAddresses = replicaAddresses;
        }

        public async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                Log.Info($"passed {sw.ElapsedMilliseconds}");
                var result = await ProcessRequestAsyncInternal(query, timeout);
                Log.Info($"Request with query \"{query}\" was processed in {sw.ElapsedMilliseconds} ms");
                return result;
            }
            catch (TimeoutException) 
            {
                Log.Info($"Request with query \"{query}\" was timeouted ({timeout.TotalMilliseconds} ms)");
                throw;
            }
        }

        protected abstract Task<string> ProcessRequestAsyncInternal(string query, TimeSpan timeout);
        protected abstract ILog Log { get; }

        protected static HttpWebRequest CreateRequest(string uriStr, TimeSpan timeout)
        {
            var request = WebRequest.CreateHttp(Uri.EscapeUriString(uriStr));
            request.Timeout = (int)timeout.TotalMilliseconds;
            request.Proxy = null;
            request.KeepAlive = true;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 100500;
            return request;
        }

        protected async Task<string> ProcessRequestAsync(WebRequest request)
        {
            Log.Info($"Send request with uri {request.RequestUri}");
            var timer = Stopwatch.StartNew();
            using (var response = await request.GetResponseAsync())
            {
                var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8)
                    .ReadToEndAsync();
                Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);

                return result;
            }
        }
    }
}