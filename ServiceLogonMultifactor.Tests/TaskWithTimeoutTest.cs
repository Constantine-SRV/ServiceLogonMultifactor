using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceLogonMultifactor.Tests
{
    [TestClass]
    public class TaskWithTimeoutTest
    {
        [TestMethod]
        public void ShouldThroughTimeoutException2()
        {
            var timerEvent = false;
            if (!Task.Factory.StartNew(() => { Thread.Sleep(6000); }).Wait(TimeSpan.FromSeconds(10))) timerEvent = true;
            Assert.IsFalse(timerEvent);
        }
    }
}