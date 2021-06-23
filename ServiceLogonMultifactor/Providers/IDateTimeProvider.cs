using System;

namespace ServiceLogonMultifactor.Providers
{
    public interface IDateTimeProvider
    {
        DateTime GetNow();

        DateTime GetUtcNow();
    }
}