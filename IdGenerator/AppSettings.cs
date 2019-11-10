using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdGenerator
{
    public class AppSettings
    {
        public string RedisHost { get; set; }
        public string RedisPort { get; set; }
        public int RedisDb { get; set; }
        public int LowLimit { get; set; }
        public int Interval { get; set; }
    }
}
