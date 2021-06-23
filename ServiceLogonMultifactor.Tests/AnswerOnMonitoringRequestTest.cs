using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Configs.ApplicationConfig;
using ServiceLogonMultifactor.Configs.Services.Generic;
using ServiceLogonMultifactor.Integration.Telegram;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Lookups;
using ServiceLogonMultifactor.Providers;
using ServiceLogonMultifactor.Wrappers;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class AnswerOnMonitoringRequestTest
    {
        [TestMethod]
        public void ShouldReadRequests()
        {
            // Arrange
            var mockTracing = new Mock<ITracing>();
            var fsMock = new Mock<IFileSystemProvider>();
            mockTracing.Setup(x => x.WriteError(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteFull(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteShort(It.IsAny<string>()));

            //act
            var sut = new MonitoringRequestsReader(mockTracing.Object, new TelegramGetUpdates(mockTracing.Object),
                new TelegramSimpleMessage(mockTracing.Object), new ExecuteCommandWrapper(mockTracing.Object, new Mock<IFileSystemProvider>().Object), new SystemInfoLookup(
                    mockTracing.Object,
                    new ExecuteCommandWrapper(mockTracing.Object, new Mock<IFileSystemProvider>().Object)), new TracingRender(mockTracing.Object));
            sut.ReadRequest();
            var writer = new ConfigWriter<LogonMultifactorConfig>(fsMock.Object);
            writer.WriteXml(AppState.GetCurrent().AppConfig); //что бы во время отладки не гонял по кругу старые сообщения
            //assert
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void shouldReadSeveralrequests()
        {
            var mockTracing = new Mock<ITracing>();
            mockTracing.Setup(x => x.WriteError(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteFull(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteShort(It.IsAny<string>()));

            //act
            var sut = new MonitoringRequestsReader(mockTracing.Object, new TelegramGetUpdates(mockTracing.Object),
                new TelegramSimpleMessage(mockTracing.Object), new ExecuteCommandWrapper(mockTracing.Object, new Mock<IFileSystemProvider>().Object), new SystemInfoLookup(
                    mockTracing.Object,
                    new ExecuteCommandWrapper(mockTracing.Object, new Mock<IFileSystemProvider>().Object)), new TracingRender(mockTracing.Object));

            for (var i = 0; i < 3; i++) sut.ReadRequest();
            var writer = new ConfigWriter<LogonMultifactorConfig>(new Mock<IFileSystemProvider>().Object);
            writer.WriteXml(AppState.GetCurrent().AppConfig);
            //assert
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }
    }
}