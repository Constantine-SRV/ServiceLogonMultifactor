using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLogonMultifactor.Enrichers;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class CurrentUserSettingsFillerTests
    {
        [TestMethod]
        public void ShouldFillRequestUserSettingsForUser1()
        {
            // Arrange
            var request = new UserSessionData();
            var userSession = new UserSessionDetails();
            userSession.UserQuser = "user1";
            request.UserSessionDetails = userSession;
            // Act
            var sut = new UserSessionEnricher();
            request = sut.Enrich(request);

            //assert
            Assert.IsNotNull(request.UserConfig.ChatId);
            Assert.IsTrue(request.UserConfig.WaitForAnswerSec == 20);
            Assert.IsTrue(request.UserConfig.NotDisconnectIP == "");
        }

        [TestMethod]
        public void ShouldFillRequestUserSettingsForUserUnnown()
        {
            // Arrange
            var request = new UserSessionData();
            var userSession = new UserSessionDetails();
            userSession.UserQuser = "user2";
            request.UserSessionDetails = userSession;
            // Act
            var sut = new UserSessionEnricher();
            request = sut.Enrich(request);

            //assert
            Assert.IsFalse(string.IsNullOrEmpty(request.UserConfig.ChatId));
        }
    }
}