using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyHttpClientFactory
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()//NOSONAR
                        .Enrich.FromLogContext()
                        .WriteTo.Logger(logger => logger.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug))
                        .WriteTo.Debug()
                        .WriteTo.File("loggg.txt", rollingInterval: RollingInterval.Day)
                        .CreateLogger();

            Log.Information("Application started");

            services.AddHttpClient("RemoteServer")
               .AddPolicyHandler(GetBulkHeadIsolationPolicy())
               .AddPolicyHandler(GetRetryPolicy())
               .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddControllers();

            services.AddHttpClient("RemoteServer", client =>
            {
                client.BaseAddress = new Uri(Configuration["RemoteServer:baseAddress"]);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
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

        internal IAsyncPolicy<HttpResponseMessage> GetBulkHeadIsolationPolicy()
        {
            return Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: Convert.ToInt32(Configuration["PollySettings:maxParallelization"]), maxQueuingActions: Convert.ToInt32(Configuration["PollySettings:maxQueuingActions"]),
                onBulkheadRejectedAsync: OnBulkheadRejectedAsync);
        }

        internal IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(5),
                    onBreak: (ex, timespan, context) => context.GetLogger()?.LogWarning($"Circuit Breaker (active for:{timespan} seconds) Connection break: {ex}"),
                    onReset: (context) => context.GetLogger()?.LogWarning($"Circuit Breaker Connection reset")
                   // onHalfOpen: (context) => context.GetLogger()?.LogWarning($"Circuit Breaker Connection half open") // There is no pattern for onHalfOpen with context
                    );
        }

        internal readonly HttpStatusCode[] HttpStatusCodesWorthRetrying =
            {
                HttpStatusCode.InternalServerError,
                HttpStatusCode.BadGateway,
                HttpStatusCode.GatewayTimeout,
                HttpStatusCode.Forbidden,
                HttpStatusCode.PaymentRequired,
                HttpStatusCode.TooManyRequests
            };

        internal IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() => Policy.HandleResult<HttpResponseMessage>(r => HttpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryAsync(new[] {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)},
                onRetry: (httpResponseMessage, timespan, retryAttempt, context) => context.GetLogger()?.LogWarning("Delaying for {delay}ms, then making retry {retry}.", timespan.TotalMilliseconds, retryAttempt)
                );


        private Task OnBulkheadRejectedAsync(Context context)
        {
            context.GetLogger()?.LogWarning("Bulkhead Isolation has Rejected the call");
            return Task.CompletedTask;
        }
    }
}