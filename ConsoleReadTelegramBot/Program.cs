using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ConsoleReadTelegramBot
{
    internal class Program
    {
        private static int _iCount;
        private static readonly object LockObjectCheckResponse = new object(); //для таймера
        private static Timer _timerCheckResponse;
        private static string _botId = "";
        private static readonly HttpWorker HttpWorker = new HttpWorker();
        private static int _maxMessageId;
        private static string _callBackQueres = "";

        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (args.Length < 1 || args[0].Length < 10)
            {
                Console.WriteLine("type bot ID and press enter");
                _botId = Console.ReadLine();
            }
            else
            {
                _botId = args[0];
            }

            Console.WriteLine("Press ESC to stop");
            _timerCheckResponse = new Timer {Enabled = true, Interval = 20 * 1000, AutoReset = true};
            _timerCheckResponse.Elapsed += TimerCheckResponseElapsed;
            _timerCheckResponse.Start();

            ConsoleKey k = 0;
            while (k != ConsoleKey.Escape) k = Console.ReadKey().Key;
        }

        private static void TimerCheckResponseElapsed(object sender, ElapsedEventArgs e)
        {
            lock (LockObjectCheckResponse)
            {
                _timerCheckResponse.Stop();
                if (!Task.Factory.StartNew(() => { ReadTelega(); }).Wait(TimeSpan.FromSeconds(20)))
                    Console.WriteLine("task timeout");
                _timerCheckResponse.Start();
            }
        }


        private static void ReadTelega()
        {
            _iCount++;
            try
            {
                var result = HttpWorker.GetUpdates(_botId);
                if (result.Contains("ErrorGetUpdate"))
                {
                    Console.WriteLine($"{_iCount} {DateTime.Now:MM-dd HH:mm:ss:f} {result.Substring(14)}");
                    return;
                }

                var telegaAnswer = new Root();
                try
                {
                    using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(result)))
                    {
                        var deserializer = new DataContractJsonSerializer(typeof(Root));
                        telegaAnswer = (Root) deserializer.ReadObject(ms);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                foreach (var r in telegaAnswer.result)
                    if (r.callback_query != null)
                    {
                        var id = r.callback_query.id;
                        if (!_callBackQueres.Contains(id))
                        {
                            var dateUnix = r.callback_query.message.date;
                            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            dt = dt.AddSeconds(dateUnix).ToLocalTime();
                            Console.WriteLine(
                                $"dt: {dt.ToString(CultureInfo.InvariantCulture)} button pressed chatID: {r.callback_query.@from.id} from:{r.callback_query.@from.first_name} text: {r.callback_query.data}");
                            _callBackQueres += ";" + id;
                            // Console.WriteLine(CallBackQueres);
                        }
                    }
                    else if (r.message != null && r.message.message_id > _maxMessageId)
                    {
                        var dateUnix = r.message.date;
                        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        dt = dt.AddSeconds(dateUnix).ToLocalTime();
                        Console.WriteLine(
                            $"dt: {dt.ToString(CultureInfo.InvariantCulture)} messageId:{r.update_id} chatID:{r.message.chat.id} from:{r.message.@from.first_name} text: {r.message.text}");
                        _maxMessageId = r.message.message_id;
                    }

                if (_iCount % 10 == 0) Console.WriteLine($"{_iCount} {DateTime.Now:MM-dd HH:mm:ss:f} ");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}