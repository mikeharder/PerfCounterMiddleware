using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PerfCounterMiddleware
{
    public class Startup
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UsePerfCounterMiddleware();

            app.Run(async (context) =>
            {
                // Limit throughput for testing current requests
                await _semaphoreSlim.WaitAsync();
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                finally
                {
                    _semaphoreSlim.Release();
                }

                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
