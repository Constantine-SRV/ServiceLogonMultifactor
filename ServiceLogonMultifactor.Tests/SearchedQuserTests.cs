using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Tests
{
    /*
>user1                 console             1  Активно   отсутствует 14.05.2021 15:54
PS C:\Windows\system32> quser
ПОЛЬЗОВАТЕЛЬ          СЕАНС              ID  СТАТУС  БЕЗДЕЙСТВ. ВРЕМЯ ВХОДА
>user1                 console             1  Активно   отсутствует 14.05.2021 15:54
PS C:\Windows\system32> quser
ПОЛЬЗОВАТЕЛЬ          СЕАНС              ID  СТАТУС  БЕЗДЕЙСТВ. ВРЕМЯ ВХОДА
>user1                 console             1  Активно   отсутствует 14.05.2021 15:54
PS C:\Windows\system32> quser
ПОЛЬЗОВАТЕЛЬ          СЕАНС              ID  СТАТУС  БЕЗДЕЙСТВ. ВРЕМЯ ВХОДА
>user1                 rdp-tcp#1           1  Активно          .  14.05.2021 15:54

USERNAME              SESSIONNAME        ID  STATE   IDLE TIME  LOGON TIME
>corta                 console             1  Active    1+06:20  2021-05-13 0:15

 */
    public class FakeTracing : ITracing
    {
        public void WriteError(string text)
        {
            // throw new NotImplementedException();
        }

        public void WriteErrorFull(string text)
        {
            // throw new NotImplementedException();
        }

        public void WriteFull(string text)
        {
            // throw new NotImplementedException();
        }

        public void WriteLine(string dir, string format, params object[] args)
        {
            // throw new NotImplementedException();
        }

        public void WriteShort(string text)
        {
            // throw new NotImplementedException();
        }

        public void WriteFullFull(string text)
        {
        }
    }

    public class FakeExeCmd : IExecuteCommandWrapper
    {
        private readonly List<string> sampleResponse;

        public FakeExeCmd(List<string> sampleResponse)
        {
            this.sampleResponse = sampleResponse;
        }

        public List<string> Execute(string command, string args)
        {
            return sampleResponse;
        }

        public string ExecuteAndCollectOutput(string command, string args)
        {
            throw new NotImplementedException();
        }
    }


    [TestClass]
    public class SearchedQuserTests
    {
        [TestMethod]
        public void ShouldParseEnglishResponseFromExeCmd()
        {
            // Arrange
            var responseExample = new List<string>
            {
                "USERNAME              SESSIONNAME        ID  STATE   IDLE TIME  LOGON TIME",
                "corta                 console             1  Active    1 + 06:20  2021-05-13 0:15"
            };
            //  IExecCMD execCmd = new FakeExeCmd(responseExample);
            // ITracing tracing = new FakeTracing();
            // Act
            var
                a = AppState.GetCurrent().AppConfig
                    .LOGON_TIME_fieldName; //FileNotFoundException: Could not find file 'C:\Users\1\source\repos\ServiceLogonMultifactor\ServiceLogonMultifactor.Tests\bin\Debug\Service.Config.xml'.
            var sut = new QueryUserLookup(new FakeExeCmd(responseExample),
                new FakeTracing()); //SearcherQuser(execCmd,tracing);
            var res = sut.Query(1);

            // Assert
            Assert.IsTrue(res.IsConsole);
            Assert.IsTrue(DateTime.Parse(res.DTquser).Year == 2021);
        }

        [TestMethod]
        public void ShouldParseRussianResponseFromExeCmd()
        {
            // Arrange
            var responseExample = new List<string>
            {
                "ПОЛЬЗОВАТЕЛЬ          СЕАНС              ID  СТАТУС  БЕЗДЕЙСТВ. ВРЕМЯ ВХОДА",
                "user1                 rdp-tcp#1           1  Активно          .  14.05.2021 15:54"
            };
            //  IExecCMD execCmd = new FakeExeCmd(responseExample);
            // ITracing tracing = new FakeTracing();
            // Act
            var sut = new QueryUserLookup(new FakeExeCmd(responseExample),
                new FakeTracing()); //SearcherQuser(execCmd,tracing);
            var res = sut.Query(1);

            // Assert
        }

        [TestMethod]
        public void ShouldSupportInvalidExecCmdResponse()
        {
            // Arrange
            var responseExample = new List<string>
            {
                "USERNAME              SESSIONNAME        ID  STATE   IDLE TIME  LOGON TIME",
                "corta                 console             1  Active    1 + 06:20  2021 - 05 - 13 0:15"
            };
            //  IExecCMD execCmd = new FakeExeCmd(responseExample);
            // ITracing tracing = new FakeTracing();
            // Act
            var sut = new QueryUserLookup(new FakeExeCmd(responseExample),
                new FakeTracing()); //SearcherQuser(execCmd,tracing);
            sut.Query(1);

            // Assert
        }
    }
}