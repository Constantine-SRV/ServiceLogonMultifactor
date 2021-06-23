using System;
using System.Collections.Generic;
using System.Linq;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Lookups
{
    public class TaskListLookup : ITaskListLookup
    {
        private readonly IExecuteCommandWrapper executeCommandWrapper;
        private readonly ITracing tracing;

        public TaskListLookup(ITracing tracing, IExecuteCommandWrapper executeCommandWrapper)
        {
            this.tracing = tracing;
            this.executeCommandWrapper = executeCommandWrapper;
        }

        public string Query(int linesPerSession = 10)
        {
            /*execute cmd
             * check 1,2,3 for ===
             * by spices in == == find begins and ends
             * convert string to int (remove letters)
             * mem usage processing 
             * fill list
             * sort 
             * for each if session not change and >10 delete elements
             * convert list to string
             */
            var lines = executeCommandWrapper.Execute("tasklist.exe", ""); //| sort / R / +58
            var lineDefinePositionNumber = 0;
            for (var i = 0; i < 3; i++)
                if (lines[i].Length > 4 && lines[i].Substring(0, 3) == "===")
                {
                    lineDefinePositionNumber = i;
                    break;
                }

            var positionPID = lines[lineDefinePositionNumber].IndexOf(" ");
            var positionSessionName = lines[lineDefinePositionNumber].IndexOf(" ", positionPID + 1);
            var positionSessionID = lines[lineDefinePositionNumber].IndexOf(" ", positionSessionName + 1);
            var positionMemUsage = lines[lineDefinePositionNumber].IndexOf(" ", positionSessionID + 1);
            var result = "";
            var collection = new List<UserSessionTaskList>();
            try
            {
                for (var i = lineDefinePositionNumber + 1; i < lines.Count; i++)
                    try
                    {
                        var tasklistResult = new UserSessionTaskList();
                        tasklistResult.ImageName = lines[i].Substring(0, positionPID).Trim();
                        tasklistResult.SessionName = lines[i]
                            .Substring(positionSessionName, positionSessionID - positionSessionName).Trim();
                        tasklistResult.SessionId = int.Parse(lines[i]
                            .Substring(positionSessionID, positionMemUsage - positionSessionID).Trim());
                        tasklistResult.MemUsageString = lines[i].Substring(positionMemUsage).Trim();
                        var memUsageInt = 0;
                        var onlyDigits =
                            new string(tasklistResult.MemUsageString.Where(c => char.IsDigit(c)).ToArray());
                        if (int.TryParse(onlyDigits, out memUsageInt))
                        {
                            tasklistResult.MemUsageInt = memUsageInt;
                            collection.Add(tasklistResult);
                        }
                    }
                    catch (Exception e)
                    {
                        tracing.WriteError($"error GetTaskList first for {lines[i]} {e.Message} ");
                    }

           
                collection = collection.OrderBy(x => x.SessionId).ThenByDescending(x => x.MemUsageInt).ToList();
                var prevSession = 0;
                var sessionRecord = 0;
                foreach (var t in collection)
                    try
                    {
                        if (prevSession == t.SessionId)
                        {
                            sessionRecord++;
                        }
                        else
                        {
                            sessionRecord = 0;
                            prevSession = t.SessionId;
                        }

                        if (sessionRecord <= linesPerSession)
                            result +=
                                $"{t.SessionId} {t.SessionName} {t.ImageName} {t.MemUsageString} {Environment.NewLine}";
                    }
                    catch (Exception e)
                    {
                        tracing.WriteError($"error GetTaskList second for  {e.Message} ");
                    }
            }
            catch (Exception e)
            {
                tracing.WriteError($"Error GetTaskList  {e.Message}");
            }

            return result;
        }
    }
}