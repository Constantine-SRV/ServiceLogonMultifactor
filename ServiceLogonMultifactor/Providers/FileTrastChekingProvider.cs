using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ServiceLogonMultifactor.Providers
{
    public class FileTrastChekingProvider: IFileTrastChekingProvider
    {
        private const string certString= "DF986A18212183B5B82CD69EA8DAFC71E30AACFC";
        private const string md5String= "44643FD4D9CE7CF1B77703D9CFEB0992";
        public bool FileBlockValid(string fileExec)
        {
            bool valid = false;
            if (CheckCert(fileExec) == certString)
            {
                if (CheckMD5(fileExec) == md5String) valid = true; else valid = false;
            }
            return valid;
        }

        private string CheckCert(string fileName)
        {
            string sertString = "00000000000000000";
            bool valid = false;
            try
            {
                //https://stackoverflow.com/questions/24060009/checking-digital-signature-on-exe
                var cert = X509Certificate.CreateFromSignedFile(fileName);
                var cert2 = new X509Certificate2(cert.Handle);
                valid = cert2.Verify();
                if (valid) sertString = cert.GetCertHashString();
            }
            catch 
            { 
            }
            return sertString;
        }

        private string CheckMD5(string fileName)
        {
            string md5return = "000000000000000000";
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(fileName))
                    {
                        md5return= BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
                    }
                }
            }
            catch { }
            return md5return;
        }

    }
}
