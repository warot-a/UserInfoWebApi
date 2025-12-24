using Microsoft.Extensions.Logging;

namespace UserInfoWebApi.Logger

{
    public class UserInfoLoggerProvider : ILoggerProvider
    {
        private readonly MiddlewareContextAccessor _middlewareContextAccessor;

        public UserInfoLoggerProvider(MiddlewareContextAccessor middlewareContextAccessor)
        {
            _middlewareContextAccessor = middlewareContextAccessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new UserInfoLogger(_middlewareContextAccessor);
        }

        public void Dispose()
        {}
    }

    public class UserInfoLogger : ILogger
    {
        private readonly MiddlewareContextAccessor _middlewareContextAccessor;

        public UserInfoLogger(MiddlewareContextAccessor middlewareContextAccessor)
        {
            _middlewareContextAccessor = middlewareContextAccessor;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            try
            {
                var traceId = _middlewareContextAccessor.GetTraceId();
                var serviceFullname = _middlewareContextAccessor.GetServiceFullname();
                var sdkVersion = _middlewareContextAccessor.GetSdkVersion();
                var path = _middlewareContextAccessor.GetPath();

                var prefix = $"traceid:{traceId}; {serviceFullname}-{sdkVersion}; Path:{path}";
                if (string.IsNullOrWhiteSpace(sdkVersion))
                {
                    prefix = $"traceid:{traceId}; {serviceFullname}; Path:{path}";
                }
                
                var serilogLevel = logLevel switch
                {
                    LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
                    LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
                    LogLevel.Information => Serilog.Events.LogEventLevel.Information,
                    LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
                    LogLevel.Error => Serilog.Events.LogEventLevel.Error,
                    LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
                    _ => Serilog.Events.LogEventLevel.Information,
                };

                Serilog.Log.Write(serilogLevel, exception, "{Prefix} - {Message}", prefix, formatter(state, exception));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error occurred while logging");
            }
        }
    }
}
