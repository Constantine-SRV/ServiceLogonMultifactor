using System;
using System.IO;
using System.Linq;

namespace ServiceLogonMultifactor.Logging.Collectors
{
    public class ErrorsCollector : IErrorsCollector
    {
        public string Collect(int days = 0)
        {
            var result = "";
            var currentFileFolder = AppDomain.CurrentDomain.BaseDirectory;
            var directory = new DirectoryInfo(Path.Combine(currentFileFolder, "log", "errors"));
            try
            {
                var logfileName = (from f in directory.GetFiles() orderby f.LastWriteTime descending select f).First();
                using (var fs = new FileStream(logfileName.FullName, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        while (sr.Peek() >= 0) // reading the old data
                            result += sr.ReadLine() + Environment.NewLine;
                    }
                }

                ;
            }
            catch (Exception e)
            {
                result = e.Message;
            }

            return result;
        }
    }
}