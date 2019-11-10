using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdGen;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace IdentifyRedisGenerator
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private const string List = "id";
        private readonly ILogger<TimedHostedService> _logger;
        private readonly System.Timers.Timer _timer;
        private readonly IdGenerator _idGenerator;
        private readonly IDatabase _db;

        public TimedHostedService(ILogger<TimedHostedService> logger, IdGenerator idGenerator, IDatabase db)
        {
            _logger = logger;
            _idGenerator = idGenerator;
            _db = db;
            _timer = new System.Timers.Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = 24 * 60 * 60 * 1000 // This make the first service call don't have to wait too long
            };
            _timer.Elapsed += _timer_Elapsed; ;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_timer.Enabled) _timer.Stop();
            try
            {
                if (_db.ListLength(List) <= 2097152)
                {
                    _logger.LogInformation("Generate 1048574 ids.");
                    var ids = _idGenerator.Take(1048574).Select(s => (RedisValue)s).ToArray();
                    _db.ListLeftPush(List, ids);
                }
                else
                {
                    _logger.LogInformation("Does not need generate ids.");
                }
            }
            finally
            {
                //_timer.Interval = 60 * 1000;
                _timer.Start();
            }
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}