using Microsoft.VisualStudio.TestTools.UnitTesting;
using SettingsSLMF;
using System;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class UserSettingsReaderTests
    {
        [TestMethod]
      
        public void ShouldReadConfigFromFile()
        {
            // Arrange

            // Act
            var sut = new UserSettingsReader();
            var usClass = sut.Read();
            // Assert
            
            Assert.AreEqual(2, usClass.Users.Count);

        }
    }
}
