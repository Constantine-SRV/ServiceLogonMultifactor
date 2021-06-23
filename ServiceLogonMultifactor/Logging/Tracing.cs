using System;
using System.Collections;
using System.IO;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging.Infrastructure;

namespace ServiceLogonMultifactor.Logging
{
    public class Tracing : ITracing, IStateAccessible
    {
        private readonly ITracingFoldersConfigurator tracingFoldersConfigurator;
       
        private TextWriter logFileWriter;
        //private readonly FileAndFolderProcessor fileAndFolderProcessor;

        public Tracing(ITracingFoldersConfigurator tracingFoldersConfigurator)
        {
            this.tracingFoldersConfigurator = tracingFoldersConfigurator;
            this.tracingFoldersConfigurator.CreateFolders();
        }

        public void WriteError(string text)
        {
            WriteFull(text);
            if (LogLevel(0)) WriteLine("errors", TextAddTime(text));
        }

        public void WriteErrorFull(string text)
        {
            WriteFull(text);

            if (LogLevel(1)) WriteLine("errors", TextAddTime(text));
        }

        public void WriteShort(string text)
        {
            WriteFull(text);
            if (LogLevel(2)) WriteLine("short", TextAddTime(text));
        }

        public void WriteFull(string text)
        {
            if (LogLevel(3)) WriteLine("full", TextAddTime(text));
        }

        public void WriteFullFull(string text)
        {
            if (LogLevel(5)) WriteLine("full", TextAddTime(text));
        }

        public void WriteLine(string dir, string format, params object[] args)
        {
            try
            {
                OpenLog(dir);

                format = format.Replace("{", "{{").Replace("}", "}}"); //to remove error in json string
                logFileWriter.WriteLine(format, args);
                CloseLog();
            }
            catch
            { //probably add event viewer
            }
        }

        private bool LogLevel(byte i)
        {
            try
            {
                var logLevelArr = new BitArray(new[] {this.GetAppConfig().LogsLevel});
                return logLevelArr[i];
            }
            catch (Exception e)
            {
                WriteLine("errors", TextAddTime(e.Message));
                return true;
            }
        }

        private string TextAddTime(string text)
        {
            return DateTime.Now.ToString("HH:mm:ss.f") + ": " + text;
        }

        private void OpenLog(string dir)
        {
            try
            {
                var currentFileFolder = AppDomain.CurrentDomain.BaseDirectory;
                var fullDir = Path.Combine(currentFileFolder, "log", dir);
                var fileName = $@"{fullDir}\log{dir}-{DateTime.Now:yyyy-MM-dd}.txt";
                logFileWriter = new StreamWriter(fileName, true);
            }
            catch (Exception e)
            {
                var currentFileFolder = AppDomain.CurrentDomain.BaseDirectory;
                var fileName = $@"{currentFileFolder}\startErrors{DateTime.Now:yyyy-MM-dd}.txt";
                var errorWriter = new StreamWriter(fileName, true);
                errorWriter.WriteLine($"{DateTime.Now:HH:mm:ss.f} {e.Message}");
                errorWriter.Flush();
                errorWriter.Close();
            }
        }

        private void CloseLog()
        {
            try
            {
                logFileWriter.Flush();
                logFileWriter.Close();
            }
            catch
            {
            }
        }
    }
}