namespace ServiceLogonMultifactor.Logging.Collectors
{
    public interface IHealthStatisticCollector
    {
        void ReportOk();
        
        void ReportFailed();
        void ReportFaileMax();
        double GetFailOkRatio(long failCo);

        string GetOkFailRatioAsText();
    }
}