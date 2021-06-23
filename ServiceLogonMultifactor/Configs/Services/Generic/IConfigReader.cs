using System;

namespace ServiceLogonMultifactor.Configs.Services.Generic
{
    public interface IConfigReader<T> where T: class, IConfigWithLastReadField
    {
        T ReadFromXmlFile(string fileName = "");

        DateTime GetLastWriteTime(string fileName = "");
    }
}