using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Configs.Services.Generic
{
    public class ConfigWriter<T> : IConfigWriter<T> where T: class
    {
        private readonly IFileSystemProvider fileSystemProvider;
        private const string DefaultConfigFileName =  "Service.Config.xml";

        public ConfigWriter(IFileSystemProvider fileSystemProvider)
        {
            this.fileSystemProvider = fileSystemProvider;
        }
        public void WriteXml(T configToWrite, string fileName = "")
        {
            var currentFileFolder = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = Path.Combine(currentFileFolder, DefaultConfigFileName);
            }
            var serializer = new XmlSerializer(typeof(T));
            var xmlWriterSettings = new XmlWriterSettings {Indent = true, NewLineOnAttributes = true};
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, xmlWriterSettings))
            {
                serializer.Serialize(xmlWriter, configToWrite);
            }

            this.fileSystemProvider.WriteAllText(fileName, sb.ToString());
        }
    }
}