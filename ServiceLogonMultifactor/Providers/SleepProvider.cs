using System;
using System.Threading;

namespace ServiceLogonMultifactor.Providers
{
    public class SleepProvider : ISleepProvider
    {
        public void Sleep(int millSecondsAmount)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(millSecondsAmount));
        }
    }
}