using System;
using System.IO;

namespace ServiceLogonMultifactor.Logging.Infrastructure
{
    public class TracingFoldersConfigurator : ITracingFoldersConfigurator
    {
        public void CreateFolders()
        {
            
            string[] foldersArr = {"short", "full", "errors"};
            var currentFileFolder = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var s in foldersArr)
            {
                var dir = Path.Combine(currentFileFolder, "log", s);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }
    }
}