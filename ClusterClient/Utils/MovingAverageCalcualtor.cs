namespace ClusterClient.Utils
{
    class MovingAverageCalcualtor
    {
        private readonly double alpha;
        private bool firstSample = true;
        private readonly object syncObject = new object();

        public MovingAverageCalcualtor(int smoothingInterval)
        {
            alpha = 1.0 / smoothingInterval;
        }

        public double Average { get; private set; } = double.PositiveInfinity;

        public void AddNextSample(double sample)
        {
            lock (syncObject)
            {
                if (firstSample)
                {
                    Average = sample;
                    firstSample = false;
                }
                else
                    Average = sample * alpha + (1 - alpha) * Average;
            }
        }
    }
}
