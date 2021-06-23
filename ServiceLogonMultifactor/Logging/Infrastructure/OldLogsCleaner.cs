using System;
using System.IO;
using System.Linq;
using ServiceLogonMultifactor.App;

namespace ServiceLogonMultifactor.Logging.Infrastructure
{
    public class OldLogsCleaner : IOldLogsCleaner, IStateAccessible
    {
       
        public void DeleteOldLogs(bool ShortLogDelete = false, bool errorLogDelete = false, bool fullLogDelete = true)
        {
            var daysOld = 0;
            try
            {
                daysOld = this.GetAppConfig().DaysOldDeliteLogs;
            }
            catch
            {
            }

            if (ShortLogDelete) DellFilesFromFolder("short", daysOld);
            if (errorLogDelete) DellFilesFromFolder("errors", daysOld);
            if (fullLogDelete) DellFilesFromFolder("full", daysOld);
        }

        private void DellFilesFromFolder(string folderName, int daysOld)
        {
            try
            {
                var currentFileFolder = AppDomain.CurrentDomain.BaseDirectory;
                var dir = Path.Combine(currentFileFolder, "log", folderName);
                var i = 0;

                var info = new DirectoryInfo(dir);
                var files = info.GetFiles("log*.*").ToArray();
                foreach (var file in files)
                {
                    var dt = file.CreationTime;
                    var d = (DateTime.Now - dt).TotalDays;
                    if (d >= daysOld)
                    {
                        i++;
                        file.Delete();
                    }
                }
                // tracing.WriteShort($"deleted {i} old files");
            }
            catch (Exception)
            {
            }
        }
    }
}