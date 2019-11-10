using System;
using IdGen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace IdGenerator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton(c => new IdGen.IdGenerator(0, new MaskConfig(42, 0, 21)));
            services.AddSingleton(c =>
            {
                var appSettings = c.GetService<IOptions<AppSettings>>();
                return ConnectionMultiplexer.Connect($"{appSettings.Value.RedisHost}:{appSettings.Value.RedisPort}");

            });
            services.AddTransient(c =>
            {
                var appSettings = c.GetService<IOptions<AppSettings>>();
                return c.GetService<ConnectionMultiplexer>().GetDatabase(appSettings.Value.RedisDb);
            });
            services.Configure<AppSettings>(option =>
            {
                option.RedisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ??
                                   throw new InvalidOperationException();
                option.RedisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ??
                                   throw new InvalidOperationException();
                option.RedisDb = int.Parse(Environment.GetEnvironmentVariable("REDIS_DB") ??
                                           throw new InvalidOperationException());
                option.LowLimit = int.Parse(Environment.GetEnvironmentVariable("LOW_LIMIT") ??
                                            throw new InvalidOperationException());
                option.Interval = int.Parse(Environment.GetEnvironmentVariable("INTERVAL") ??
                                            throw new InvalidOperationException());
            });
            services.AddHostedService<TimedHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
