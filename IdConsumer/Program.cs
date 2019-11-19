using System;
using StackExchange.Redis;

namespace IdConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = ConnectionMultiplexer.Connect("debian-dev").GetDatabase(0);
            while (true)
            {
                Console.Clear();
                var items = (long)db.ListLeftPop("id");
                Console.WriteLine(items);
                TextCopy.Clipboard.SetText(items.ToString());
                var q = Console.ReadLine();
                if (q?.ToLower() == "q")
                {
                    break;
                }
            }
        }
    }
}
