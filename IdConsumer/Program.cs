using System;
using System.Threading;
using StackExchange.Redis;

namespace IdsConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = ConnectionMultiplexer.Connect("debian-dev").GetDatabase(1);
            long pre = 0;
            for (int i = 0; i < 10000000; i++)
            {
                var items = (long)db.ListRightPop("id");
                if (items <= pre)
                {
                    throw new Exception();
                }

                pre = items;
                Console.WriteLine(items);
            }
        }
    }
}
