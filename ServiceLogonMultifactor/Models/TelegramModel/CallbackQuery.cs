namespace ServiceLogonMultifactor.Models.TelegramModel
{
    public class CallbackQuery
    {
        public string id { get; set; }
        public From from { get; set; }
        public Message message { get; set; }
        public string chat_instance { get; set; }
        public string data { get; set; }
    }
}