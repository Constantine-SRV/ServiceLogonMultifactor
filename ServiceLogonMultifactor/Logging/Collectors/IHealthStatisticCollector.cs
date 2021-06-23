namespace ServiceLogonMultifactor.Logging.Collectors
{
    public interface IHealthStatisticCollector
    {
        void ReportOk();
        
        void ReportFailed();
        void ReportFailed5();
        double GetFailOkRatio(long failCo);

        string GetOkFailRatioAsText();
    }
}