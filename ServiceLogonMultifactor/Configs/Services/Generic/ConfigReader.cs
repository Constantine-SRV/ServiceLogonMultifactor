using System;
using System.IO;
using System.Xml.Serialization;
using ServiceLogonMultifactor.Logging;
using ServiceLogonMultifactor.Providers;

namespace ServiceLogonMultifactor.Configs.Services.Generic
{
    public class ConfigReader<T> : IConfigReader<T> where T : class, IConfigWithLastReadField, new()
    {
        private readonly ITracing tracing;
        private readonly IFileSystemProvider fileSystemProvider;

        private const string DefaultConfigFileName =  "Service.Config.xml";

        public ConfigReader(ITracing tracing, IFileSystemProvider fileSystemProvider)
        {
            this.tracing = tracing;
            this.fileSystemProvider = fileSystemProvider;
        }

        public T ReadFromXmlFile(string fileName = "")
        {
            var config = new T();
            var currentFileFolder = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(fileName))
            {
               
                fileName = Path.Combine(currentFileFolder, DefaultConfigFileName);
            }
            try
            {
                var fileContent = fileSystemProvider.ReadAllText(fileName);
                using (var sr = new StringReader(fileContent))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    config = serializer.Deserialize(sr) as T;
                }

                if (config == null)
                {
                    throw new ArgumentNullException(nameof(config),
                        "Unable to deserialize config from xml. Please check xml");
                }
                
                config.LastConfigRead = File.GetLastWriteTime(fileName);
            }
            catch (Exception e)
            {
                tracing.WriteError($"error read settings {e.Message}");
            }

            return config;
        }

        public DateTime GetLastWriteTime(string fileName = "")
        {
            var currentFileFolder = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName =  Path.Combine(currentFileFolder, DefaultConfigFileName);
            }


            return fileSystemProvider.GetLastWriteTime(fileName);
        }
    }
}