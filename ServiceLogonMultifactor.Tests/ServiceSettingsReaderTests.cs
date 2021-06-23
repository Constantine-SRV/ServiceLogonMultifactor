using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Configs.Services.Generic;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class ServiceSettingsReaderTests
    {
        [TestMethod]
        public void ShouldReadConfigFromFile()
        {
            // Arrange
            var mockTracing = new Mock<ITracing>();
            var fsProviderMock = new Mock<IFileSystemProvider>();

            mockTracing.Setup(x => x.WriteError(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteFull(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteShort(It.IsAny<string>()));
            
            // Act
            var sut = new ConfigReader<LogonMultifactorConfig>(mockTracing.Object, fsProviderMock.Object);
            var serviceConfig = sut.ReadFromXmlFile();
            serviceConfig.SingleServiceOnTheBot = false;

            var writer = new ConfigWriter<LogonMultifactorConfig>(fsProviderMock.Object);
            writer.WriteXml(serviceConfig);
            // Assert


            Assert.IsTrue(serviceConfig.UsersCollectionSection.UserConfigs.Count >= 3);
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }
    }
}