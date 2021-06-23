using System.Collections.Generic;

namespace ServiceLogonMultifactor.Wrappers
{
    public interface IExecuteCommandWrapper
    {
        string ExecuteAndCollectOutput(string command, string args);
        List<string> Execute(string command, string args);
    }
}