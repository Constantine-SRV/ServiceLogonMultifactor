using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Logging.Infrastructure;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class TracingTest
    {
        [TestMethod]
        public void WtiteError()
        {
            // Arrange
            // Act
            var sut = new Tracing(new TracingFoldersConfigurator());
            sut.WriteErrorFull("test");


            // Assert


            //Assert.IsTrue(serviceConfig.Users.User.Count >= 3);
        }
    }
}