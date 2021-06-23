using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Logging
{
    public interface ITracingRender
    {
        string RenderUserSessionDetails(UserSessionDetails userSessionDetails);
        string RenderUserSessionData(UserSessionData userSessionData, int eventNumber);
        string RenderUserSettings(UserConfig userConfig);
        string RenderConfig(LogonMultifactorConfig logonMultifactorConfig);
    }
}