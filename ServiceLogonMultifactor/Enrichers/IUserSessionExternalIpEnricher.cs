using ServiceLogonMultifactor.Models.UserSessionModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceLogonMultifactor.Enrichers
{
    interface IUserSessionExternalIpEnricher : IEnricher<UserSessionDetails>
    {
    }
}
