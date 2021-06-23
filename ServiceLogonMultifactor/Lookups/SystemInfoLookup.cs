using System;
using System.Linq;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Lookups
{
    public class SystemInfoLookup : ISystemInfoLookup
    {
        private readonly IExecuteCommandWrapper executeCommandWrapper;
        private readonly ITracing tracing;

        public SystemInfoLookup(ITracing tracing, IExecuteCommandWrapper executeCommandWrapper)
        {
            this.tracing = tracing;
            this.executeCommandWrapper = executeCommandWrapper;
        }

        public string Query(string systemInfoFields = "")
        {
            var result = "";
            try
            {
                tracing.WriteFull($"SystemInfo param  {systemInfoFields}");
                if (systemInfoFields.Length == 0) return result;
                var systemInfoFieldsArr = systemInfoFields.Split(';');
                var firstString = 0;
                var lines = executeCommandWrapper.Execute("systeminfo.exe", "/fo table");
                if (lines.Count == 4) firstString = 1; //sometime there is empty string at the beginning
                if (lines.Count < 3) return result;
                var line1 = lines[firstString];
                var line2 = lines[firstString + 1];
                var line3 = lines[firstString + 2];

                for (var i = 0; i < systemInfoFieldsArr.Count(); i++)
                {
                    var beginPosition =
                        line1.IndexOf(systemInfoFieldsArr[i], StringComparison.InvariantCultureIgnoreCase);
                    var endPosition = line2.IndexOf(' ', beginPosition + 1);
                    var paramName = line1.Substring(beginPosition, endPosition - beginPosition).Trim();
                    var paramValue = line3.Substring(beginPosition, endPosition - beginPosition).Trim();
                    result += $"{paramName}: {paramValue}{Environment.NewLine}";
                }
            }
            catch (Exception e)
            {
                tracing.WriteError($"SystemInfoResult error  {e.Message}");
            }

            return result;
        }
    }
}