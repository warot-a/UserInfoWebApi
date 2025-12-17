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
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
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
                var message = $"{prefix} - {formatter(state, exception)}";
                Console.WriteLine(message); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while logging: {ex.Message}");
            }
        }
    }
}
