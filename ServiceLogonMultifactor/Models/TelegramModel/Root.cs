using System.Collections.Generic;

namespace ServiceLogonMultifactor.Models.TelegramModel
{
    public class Root
    {
        public bool ok { get; set; }
        public List<Result> result { get; set; }
    }
}