using System.Collections.Generic;

namespace ServiceLogonMultifactor.Models.TelegramModel
{
    public class Message
    {
        public int message_id { get; set; }
        public From from { get; set; }
        public Chat chat { get; set; }
        public int date { get; set; }
        public string text { get; set; }
        public List<Entity> entities { get; set; }
        public ReplyMarkup reply_markup { get; set; }
    }
}