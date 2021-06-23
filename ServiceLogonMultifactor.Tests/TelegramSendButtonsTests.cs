using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLogonMultifactor.App;
using ServiceLogonMultifactor.Enrichers;
using ServiceLogonMultifactor.Integration.Telegram;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class TelegramSendButtonsTests
    {
        private readonly Mock<ITracing> mockTracing = new Mock<ITracing>();

        public TelegramSendButtonsTests()
        {
            MocInit();
        }

        private void MocInit()
        {
            mockTracing.Setup(x => x.WriteError(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteFull(It.IsAny<string>()));
            mockTracing.Setup(x => x.WriteShort(It.IsAny<string>()));
        }

        [TestMethod]
        public void ShouldSendButtons()
        {
            // Arrange

            var
                a = AppState.GetCurrent().AppConfig
                    .LOGON_TIME_fieldName; //FileNotFoundException: Could not find file 'C:\Users\1\source\repos\ServiceLogonMultifactor\ServiceLogonMultifactor.Tests\bin\Debug\Service.Config.xml'.
            var telegramTexts = new TelegramTexts(mockTracing.Object);
            var sut = new TelegramButtons(mockTracing.Object, telegramTexts);
            ; //SearcherQuser(execCmd,tracing);
            var userSession = new UserSessionDetails();
            userSession.IP = "192.168.110.129";
            userSession.IsConsole = true;
            userSession.UserQuser = "ivanov.a";
            var request = new UserSessionData();
            request.IdRequest = "20210524142332_TERM2008R2_8";
            request.UserSessionDetails = userSession;

            request = new UserSessionEnricher().Enrich(request);
            sut.SendButtons(request);

            // Assert
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void ShouldSendMessageUserInThelistNotallowTochangeIP()
        {
            // Arrange

            var
                a = AppState.GetCurrent().AppConfig
                    .LOGON_TIME_fieldName; //FileNotFoundException: Could not find file 'C:\Users\1\source\repos\ServiceLogonMultifactor\ServiceLogonMultifactor.Tests\bin\Debug\Service.Config.xml'.
            var telegramTexts = new TelegramTexts(mockTracing.Object);
            var sut = new TelegramButtons(mockTracing.Object, telegramTexts);
            ; //SearcherQuser(execCmd,tracing);
            var userSession = new UserSessionDetails();
            userSession.IP = "10.10.10.0";
            userSession.IsConsole = false;
            userSession.UserQuser = "user2";
            var request = new UserSessionData();
            request.IdRequest = "888-99-000-123";
            request.UserSessionDetails = userSession;

            request = new UserSessionEnricher().Enrich(request);
            sut.SendButtons(request);

            // Assert
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void ShouldSendMessageUserNOTInThelist()
        {
            // Arrange

            var
                a = AppState.GetCurrent().AppConfig
                    .LOGON_TIME_fieldName; //FileNotFoundException: Could not find file 'C:\Users\1\source\repos\ServiceLogonMultifactor\ServiceLogonMultifactor.Tests\bin\Debug\Service.Config.xml'.
            var telegramTexts = new TelegramTexts(mockTracing.Object);
            var sut = new TelegramButtons(mockTracing.Object, telegramTexts);
            ; //SearcherQuser(execCmd,tracing);
            var userSession = new UserSessionDetails();
            userSession.IP = "1.1.1.1";
            userSession.IsConsole = false;
            userSession.UserQuser = "user88";
            userSession.UserName = @"test\user88";
            var request = new UserSessionData();
            request.IdRequest = "888-99-000-123";
            request.UserSessionDetails = userSession;

            request = new UserSessionEnricher().Enrich(request);
            sut.SendButtons(request);

            // Assert
            mockTracing.Verify(x => x.WriteError(It.IsAny<string>()), Times.Never);
        }
    }
}