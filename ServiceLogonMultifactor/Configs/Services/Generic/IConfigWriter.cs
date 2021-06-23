namespace ServiceLogonMultifactor.Configs.Services.Generic
{
    public interface IConfigWriter<T> where T: class
    {
        void WriteXml(T configToWrite, string fileName = "");
    }
}