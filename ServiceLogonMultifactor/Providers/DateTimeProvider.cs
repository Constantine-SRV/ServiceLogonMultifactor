using System;

namespace ServiceLogonMultifactor.Providers
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetNow() => DateTime.Now;

        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}