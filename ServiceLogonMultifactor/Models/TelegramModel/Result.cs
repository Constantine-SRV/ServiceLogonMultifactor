namespace ServiceLogonMultifactor.Models.TelegramModel
{
    public class Result
    {
        public int update_id { get; set; }
        public Message message { get; set; }
        public CallbackQuery callback_query { get; set; }
    }
}