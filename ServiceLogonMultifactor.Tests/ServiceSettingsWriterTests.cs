using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Configs.Services.Generic;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class ServiceSettingsWriterTests
    {
        private readonly Mock<ITracing> mockTracing = new Mock<ITracing>();

        private void MocInit()
        {
            mockTracing.Setup(x => x.WriteError(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteFull(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteShort(It.IsAny<string>()));
        }

        [TestMethod]
        public void ShouldReadAndSaveConfigFile()
        {
            MocInit();
            // Arrange
            var sC = new ConfigReader<LogonMultifactorConfig>(mockTracing.Object, new Mock<IFileSystemProvider>().Object);
            var serviceConfig =
                sC.ReadFromXmlFile(
                    @"C:\Users\1\source\repos\ServiceLogonMultifactor\ServiceLogonMultifactor\bin\Release\Service.Config.xml");
            // Act

            var sut = new ConfigWriter<LogonMultifactorConfig>(new Mock<IFileSystemProvider>().Object);
            sut.WriteXml(serviceConfig, @"c:\!\test2.config.xml");
            // Assert

            Assert.IsTrue(serviceConfig.UsersCollectionSection.UserConfigs.Count >= 1);
            Assert.IsFalse(string.IsNullOrEmpty(serviceConfig.ChatId));
        }
    }
}