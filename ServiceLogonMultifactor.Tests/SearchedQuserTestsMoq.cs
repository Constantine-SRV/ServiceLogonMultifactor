using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class SearchedQuserTestsMoq
    {
        [TestMethod]
        public void TestMethod1()
        {
        }


        [TestMethod]
        public void ShouldParseEnglishResponseFromExeCmd()
        {
            // Arrange
            var responseExample = new List<string>
            {
                "USERNAME              SESSIONNAME        ID  STATE   IDLE TIME  LOGON TIME",
                "corta                 console             1  Active    1 + 06:20  2021-05-13 0:15"
            };


            var mockExecCmd = new Mock<IExecuteCommandWrapper>();
            mockExecCmd.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>())).Returns(responseExample);
            //mockExecCmd.Setup(x => x.Execute(It.Is<string>(w=>w=="abcde"), It.IsAny<string>())).Returns(responseExample);


            // Act
            var
                a = AppState.GetCurrent().AppConfig
                    .LOGON_TIME_fieldName; //FileNotFoundException: Could not find file 'C:\Users\1\source\repos\ServiceLogonMultifactor\ServiceLogonMultifactor.Tests\bin\Debug\Service.Config.xml'.
            var sut = new QueryUserLookup(mockExecCmd.Object, new FakeTracing()); //SearcherQuser(execCmd,tracing);
            var res = sut.Query(1);

            // Assert
            Assert.IsTrue(res.IsConsole);
            Assert.IsTrue(DateTime.Parse(res.DTquser).Year == 2021);
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
    }
}