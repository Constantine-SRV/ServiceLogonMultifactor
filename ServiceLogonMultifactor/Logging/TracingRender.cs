using System;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Logging
{
    public class TracingRender : ITracingRender
    {
        private readonly ITracing tracing;

        public TracingRender(ITracing tracing)
        {
            this.tracing = tracing;
        }

        public string RenderUserSessionData(UserSessionData userSessionData, int eventNumber)
        {
            var finalString = "";
            finalString +=
                $"___session settings of event {eventNumber} {Environment.NewLine}{RenderUserSessionDetails(userSessionData.UserSessionDetails)}" +
                $"Request userIndexInSettings :{userSessionData.UserIndexInSettings.ToString()} {Environment.NewLine}___userSettings : {Environment.NewLine} " +
                $"{RenderUserSettings(userSessionData.UserConfig)}";

            return finalString;
        }

        public string RenderUserSessionDetails(UserSessionDetails userSessionDetails)
        {
            //used only in login event 
            var finalString = "";
            try
            {
                foreach (var prop in userSessionDetails.GetType().GetProperties())
                    finalString +=
                        $"  {prop.Name} - {(prop.GetValue(userSessionDetails, null) == null ? "null" : prop.GetValue(userSessionDetails, null).ToString())} {Environment.NewLine}";
            }
            catch (Exception e)
            {
                finalString = "error";
                tracing.WriteError($"error UserSesionToString {e.Message}");
            }

            return finalString;
        }

        public string RenderConfig(LogonMultifactorConfig logonMultifactorConfig)
        {
            var finalString = "";
            try
            {
                foreach (var prop in logonMultifactorConfig.GetType().GetProperties())
                    finalString +=
                        $"{prop.Name} - {(prop.GetValue(logonMultifactorConfig, null) == null ? "null" : prop.GetValue(logonMultifactorConfig, null).ToString())} {Environment.NewLine}";
                finalString += $"{Environment.NewLine}Users {Environment.NewLine}";
                foreach (var u in logonMultifactorConfig.UsersCollectionSection.UserConfigs)
                    finalString += $"{RenderUserSettings(u)} --------------- {Environment.NewLine}";
            }

            catch (Exception e)
            {
                finalString = "error";
                tracing.WriteError($"Error AllSettingsToString {e.Message}");
            }

            return finalString;
        }

        public string RenderUserSettings(UserConfig userConfig)
        {
            var finalString = "";
            try
            {
                foreach (var prop in userConfig.GetType().GetProperties())
                    finalString +=
                        $"  {prop.Name} - {(prop.GetValue(userConfig, null) == null ? "null" : prop.GetValue(userConfig, null).ToString())} {Environment.NewLine}";
            }
            catch (Exception e)
            {
                finalString = "error";
                tracing.WriteError($"Error UserSettingsToString {e.Message}");
            }

            return finalString;
        }
    }
}