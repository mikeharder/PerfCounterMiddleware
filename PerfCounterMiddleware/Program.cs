using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PerfCounterMiddleware
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var writePerfCountersTask = WritePerfCounters(cts.Token);

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();

            cts.Cancel();
            writePerfCountersTask.Wait();
        }

        private static async Task WritePerfCounters(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                PerfCounterMiddleware.Write(Console.Out);
            }
        }
    }
}
