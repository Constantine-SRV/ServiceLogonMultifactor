using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Logging;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class SettingsToStringTest
    {
        [TestMethod]
        public void ShouldReadSettingsToString()
        {
            // Arrange
            var mockTracing = new Mock<ITracing>();
            mockTracing.Setup(x => x.WriteError(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteFull(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteShort(It.IsAny<string>()));

            //act
            var sut = new TracingRender(mockTracing.Object);

            var a = sut.RenderConfig(AppState.GetCurrent().AppConfig);
            //assert
            Assert.IsTrue(a.Length > 1000);
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }
    }
}