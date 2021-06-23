using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Configs.Services;
using ServiceLogonMultifactor.Configs.Services.Generic;
using ServiceLogonMultifactor.Integration.Telegram;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Providers;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class RegularTasksTest


    {
        private readonly Mock<ITracing> mockTracing = new Mock<ITracing>();

        private void MocInit()
        {
            mockTracing.Setup(x => x.WriteError(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteFull(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteShort(It.IsAny<string>()));
        }

        [TestMethod]
        public void ShouldExecuteOneMinuteTimer()
        {
            // Arrange
            AppState.GetCurrent().AppConfig.UsersCollectionSection.UserConfigs[1].NotDisconnectIP += ";10.10.10.0";
            AppState.GetCurrent().AppConfig.ThreIsNewChanges = true;
            var dt = AppState.GetCurrent().AppConfig.LastConfigRead;
            //act
            MocInit();

            var execCmd = new ExecuteCommandWrapper(mockTracing.Object, new Mock<IFileSystemProvider>().Object);
            var sut = new AppWorkers(new Mock<IUsersIpConfigManager>().Object, new FakeTracing(), new MonitoringRequestsReader(mockTracing.Object,
                new TelegramGetUpdates(mockTracing.Object),
                new TelegramSimpleMessage(mockTracing.Object), execCmd, new SystemInfoLookup(mockTracing.Object, execCmd),
                new TracingRender(mockTracing.Object)));
            sut.StartOneMinuteProcess();

            //assert
            Assert.IsTrue(dt < AppState.GetCurrent().AppConfig.LastConfigRead);
            var config = new ConfigReader<LogonMultifactorConfig>(
                mockTracing.Object,
                new Mock<IFileSystemProvider>().Object)
                .ReadFromXmlFile();
            
            Assert.IsTrue(config.UsersCollectionSection.UserConfigs[1].NotDisconnectIP
                .Contains("10.10.10.0"));

            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void ShouldChangeNotDisconnectIPAndSaveConfig()
        {
            MocInit();
            // Arrange
            AppState.GetCurrent().AppConfig.UsersCollectionSection.UserConfigs[1].NotDisconnectIP += ";10.10.10.0";
            AppState.GetCurrent().AppConfig.ThreIsNewChanges = true;

            var execCmd = new ExecuteCommandWrapper(mockTracing.Object, new Mock<IFileSystemProvider>().Object);
            //act

            var sut = new AppWorkers(new Mock<IUsersIpConfigManager>().Object,new FakeTracing(), new MonitoringRequestsReader(mockTracing.Object,
                new TelegramGetUpdates(mockTracing.Object),
                new TelegramSimpleMessage(mockTracing.Object), execCmd, new SystemInfoLookup(mockTracing.Object, execCmd),
                new TracingRender(mockTracing.Object)));
            sut.StartOneMinuteProcess();

            //assert
            var config = new ConfigReader<LogonMultifactorConfig>(
                mockTracing.Object, 
                new Mock<IFileSystemProvider>().Object)
                .ReadFromXmlFile();
            
            Assert.IsTrue(config.UsersCollectionSection.UserConfigs[1].NotDisconnectIP
                .Contains("10.10.10.0"));
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }
    }
}