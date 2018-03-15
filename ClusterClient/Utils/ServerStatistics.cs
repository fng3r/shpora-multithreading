namespace ClusterClient.Utils
{
    public class ServerStatistics
    {
        private readonly ConcurrentDefaultDictionary<string, MovingAverageCalcualtor> averages;

        public ServerStatistics()
        {
            averages = new ConcurrentDefaultDictionary<string, MovingAverageCalcualtor>(
                () => new MovingAverageCalcualtor(20));
        }

        public void AddData(string serverAddress, double data) => averages[serverAddress].AddNextSample(data);

        public double GetAverageResponseTime(string serverAddress) => averages[serverAddress].Average;
    }
}
