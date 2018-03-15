using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Utils
{
    class ClusterClientException : Exception
    {
        public ClusterClientException(string message) : base(message)
        {
        }

        public ClusterClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
