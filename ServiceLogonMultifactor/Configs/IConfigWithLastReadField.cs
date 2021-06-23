using System;

namespace ServiceLogonMultifactor.Configs
{
    public interface IConfigWithLastReadField
    {
        DateTime LastConfigRead { get; set; }
    }
}