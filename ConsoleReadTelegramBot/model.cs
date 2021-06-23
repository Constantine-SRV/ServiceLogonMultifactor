using System.Collections.Generic;

namespace ConsoleReadTelegramBot
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class From
    {
        public int id { get; set; }
        public bool is_bot { get; set; }
        public string first_name { get; set; }
        public string language_code { get; set; }
        public string last_name { get; set; }
        public string username { get; set; }
    }

    public class Chat
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string type { get; set; }
        public string last_name { get; set; }
    }

    public class Entity
    {
        public int offset { get; set; }
        public int length { get; set; }
        public string type { get; set; }
    }

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

    public class ReplyMarkup
    {
        public Inline_Keyboard[][] inline_keyboard { get; set; }
    }

    public class Inline_Keyboard
    {
        public string text { get; set; }
        public string callback_data { get; set; }
    }

    public class CallbackQuery
    {
        public string id { get; set; }
        public From from { get; set; }
        public Message message { get; set; }
        public string chat_instance { get; set; }
        public string data { get; set; }
    }

    public class Result
    {
        public int update_id { get; set; }
        public Message message { get; set; }
        public CallbackQuery callback_query { get; set; }
    }

    public class Root
    {
        public bool ok { get; set; }
        public List<Result> result { get; set; }
    }
}