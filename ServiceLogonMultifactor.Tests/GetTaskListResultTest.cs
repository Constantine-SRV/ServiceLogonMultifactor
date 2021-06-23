using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Providers;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class GetTaskListResultTest
    {
        [TestMethod]
        public void ShouldReadTaskListResult()
        {
            // Arrange
            var mockTracing = new Mock<ITracing>();
            mockTracing.Setup(x => x.WriteError(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteFull(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteShort(It.IsAny<string>()));

            //act
            var sut = new TaskListLookup(mockTracing.Object, new ExecuteCommandWrapper(mockTracing.Object, new Mock<IFileSystemProvider>().Object));

            var a = sut.Query(20);
            //assert
            Assert.IsTrue(a.Length > 1000);
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }
    }
}