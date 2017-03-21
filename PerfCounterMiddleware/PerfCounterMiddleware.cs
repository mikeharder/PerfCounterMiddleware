using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PerfCounterMiddleware
{
    public class PerfCounterMiddleware
    {
        private static long _requestsStarted = 0;
        private static long _requestsCompleted = 0;
        private static long _requestTicks = 0;

        private readonly RequestDelegate _next;

        public PerfCounterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var sw = new Stopwatch();
            try
            {
                Interlocked.Increment(ref _requestsStarted);
                sw.Start();
                await _next(httpContext);
            }
            finally
            {
                sw.Stop();
                Interlocked.Add(ref _requestTicks, sw.ElapsedTicks);
                Interlocked.Increment(ref _requestsCompleted);
            }
        }

        public static void Write(TextWriter writer)
        {
            var currentRequests = _requestsStarted - _requestsCompleted;
            var secondsPerRequest = (((double)_requestTicks) / _requestsCompleted) / Stopwatch.Frequency;

            writer.WriteLine(
                $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] " +
                $"CurrentRequests: {currentRequests}\t" +
                $"RequestsCompleted: {_requestsCompleted}\t" +
                $"SecondsPerRequest: {Math.Round(secondsPerRequest, 2)}"
            );
        }
    }

    public static class PerfCounterMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerfCounterMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerfCounterMiddleware>();
        }
    }
}
