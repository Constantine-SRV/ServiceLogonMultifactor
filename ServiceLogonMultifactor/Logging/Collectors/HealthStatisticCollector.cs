using System.Threading;

namespace ServiceLogonMultifactor.Logging.Collectors
{
    public class HealthStatisticCollector : IHealthStatisticCollector
    {
        private long okCount = 0;

        private long failedCount = 0;
        private long failedCountMax = 0;

        private HealthStatisticCollector()
        {
        }
        
        private static readonly HealthStatisticCollector Instance = new HealthStatisticCollector();

        public static IHealthStatisticCollector GetCurrent()
        {
            return Instance;
        }

        public void ReportOk()
        {
            Interlocked.Increment(ref okCount);
        }

        public void ReportFailed()

        {
            Interlocked.Increment(ref failedCount);
        }
        public void ReportFaileMax()
        {
            Interlocked.Increment(ref failedCountMax);
        }

        public double GetFailOkRatio(long failCo)
        {
            var overallCount = (okCount + (double) failedCount);
            if (overallCount == 0)
            {
                return 0;
            }
            return (double) failCo * 100 / overallCount;
        }


        public string GetOkFailRatioAsText()
        {
           return 
                $"Updates ok: {okCount} failed: {failedCount}({failedCountMax}) " +
                $"ratio {GetFailOkRatio(failedCount):0.0}({GetFailOkRatio(failedCountMax):0.0})%";
        }
    }
}