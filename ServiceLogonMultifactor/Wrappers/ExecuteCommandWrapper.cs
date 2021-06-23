using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Wrappers
{
    public class ExecuteCommandWrapper : IExecuteCommandWrapper
    {
        private readonly string pathToSystem;

        private readonly ITracing tracing;
        private readonly IFileSystemProvider fileSystemProvider;

        private const string AllowedCommands = "quser.exe tsdiscon.exe msg.exe systeminfo.exe tasklist.exe";

        public ExecuteCommandWrapper(ITracing tracing, IFileSystemProvider fileSystemProvider)
        {
            this.tracing = tracing;
            this.fileSystemProvider = fileSystemProvider;
            pathToSystem = GetSystem32Path();
        }


        public string ExecuteAndCollectOutput(string command, string args)
        {
            var sb = new StringBuilder();
            try
            {
                var lines = Execute(command, args);
                foreach (var t in lines)
                {
                    sb.AppendLine(t);
                }
            }
            catch (Exception e)
            {
                tracing.WriteError($"ExecuteToSting  {e.Message}");
            }

            return sb.ToString();
        }

        public List<string> Execute(string command, string args)
        {
            var lines = new List<string>();
            
            var commandEnabled = AllowedCommands.IndexOf(command, StringComparison.CurrentCultureIgnoreCase) >= 0;
            if (!commandEnabled)
            {
                tracing.WriteError($"command {command} not in the list");
                lines.Add($"command {command} not in the list");
                return lines;
            }

            try
            {
                if (pathToSystem == "") tracing.WriteError("error in  path to sys32");
                var fileExec = pathToSystem + command;
                tracing.WriteFull($"cmd {fileExec} {args}");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileExec,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,

                        StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage)
                    }
                };
                process.Start();
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();
                    lines.Add(line);
                }
            }
            catch (Exception e)
            {
                tracing.WriteError($"ExecCMD  {e.Message}");
            }
            return lines;
        }

        private string GetSystem32Path()
        {
            var dir = Environment.ExpandEnvironmentVariables(@"%windir%\Sysnative\");
            var fileQuser = dir + "quser.exe";
            if (!fileSystemProvider.FileExists(fileQuser))
            {
                dir = Environment.ExpandEnvironmentVariables(@"%windir%\system32\");
                fileQuser = dir + "quser.exe";
            }
            if (!fileSystemProvider.FileExists(fileQuser)) tracing.WriteError("GetSystem32Path  both paths are incorrect");
            return dir;
        }
    }
}