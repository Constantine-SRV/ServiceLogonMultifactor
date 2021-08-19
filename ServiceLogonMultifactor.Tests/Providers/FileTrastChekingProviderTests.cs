using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLogonMultifactor.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLogonMultifactor.Providers.Tests
{
    [TestClass()]
    public class FileTrastChekingProviderTests
    {
        [TestMethod()]
        public void FileBlockValidTest()
        {
            //act
            var sut = new FileTrastChekingProvider();

            var a = sut.FileBlockValid(@"C:\Users\1\source\repos\ServiceLogonMultifactor_ref\ServiceLogonMultifactor\bin\Release\formBlockAccess.exe");
            var b= sut.FileBlockValid(@"C:\Users\1\source\repos\ServiceLogonMultifactor_ref\ServiceLogonMultifactor\bin\Debug\formBlockAccess.exe");
            Assert.IsTrue(a);
            Assert.IsFalse(b);
        }
    }
}