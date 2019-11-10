using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace IdGenerator
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private const string List = "id";
        private readonly ILogger<TimedHostedService> _logger;
        private readonly System.Timers.Timer _timer;
        private readonly IdGen.IdGenerator _idGenerator;
        private readonly IDatabase _db;
        private readonly AppSettings _appSettings;

        public TimedHostedService(ILogger<TimedHostedService> logger, IdGen.IdGenerator idGenerator, IDatabase db, IOptions<AppSettings> options)
        {
            _logger = logger;
            _idGenerator = idGenerator;
            _db = db;
            _appSettings = options.Value;
            _timer = new System.Timers.Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = _appSettings.Interval * 1000
            };
            _timer.Elapsed += _timer_Elapsed;
            _timer_Elapsed(null, null);
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_timer.Enabled) _timer.Stop();
            try
            {
                if (_db.ListLength(List) <= _appSettings.LowLimit)
                {
                    _logger.LogInformation($"Generate {_appSettings.LowLimit} ids.");
                    var ids = _idGenerator.Take(_appSettings.LowLimit).Select(s => (RedisValue)s).ToArray();
                    _db.ListLeftPush(List, ids);
                }
                else
                {
                    _logger.LogInformation("Does not need generate ids.");
                }
            }
            finally
            {
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