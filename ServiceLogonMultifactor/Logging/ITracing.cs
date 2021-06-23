namespace ServiceLogonMultifactor.Logging
{
    public interface ITracing
    {
        void WriteError(string text);
        void WriteErrorFull(string text); //for conflict
        void WriteShort(string text);
        void WriteFull(string text);
        void WriteFullFull(string text);
        void WriteLine(string dir, string format, params object[] args);
    }
}