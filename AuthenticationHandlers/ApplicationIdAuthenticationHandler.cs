using System.Security.Claims;
using System.Text.Encodings.Web;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserInfoWebApi.DynamoDB;
using UserInfoWebApi.Model.Response;

namespace UserInfoWebApi.AuthenticationHandlers
{
    public static class AuthenticationConstants
    {
        public const string ApplicationIdAuthentication = "ApplicationIdAuthentication";
        public const string ApplicationIdHeader = "x-application-id";
        public const int MemoryCacheTTL = 5;
    }

    public class ApplicationIdAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string HeaderName { get; set; } = AuthenticationConstants.ApplicationIdHeader;
    }

    public class ApplicationIdAuthenticationHandler : AuthenticationHandler<ApplicationIdAuthenticationOptions>
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly IMemoryCache _memoryCache;

        public ApplicationIdAuthenticationHandler(
            IOptionsMonitor<ApplicationIdAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IDynamoDbClientFactory dynamoDbClientFactory,
            IMemoryCache memoryCache) : base(options, logger, encoder, clock)
        {
            _dynamoDbClient = dynamoDbClientFactory.CreateDynamoDbClient();
            _memoryCache = memoryCache;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Request.Headers.TryGetValue(AuthenticationConstants.ApplicationIdHeader, out var appIdToken);
            if (string.IsNullOrWhiteSpace(appIdToken))
            {
                return AuthenticateResult.Fail($"HTTP Header {AuthenticationConstants.ApplicationIdHeader} is required");
            }

            var application = GetApplicationFromCache(appIdToken);
            if (string.IsNullOrWhiteSpace(application.AppName))
            {
                application = await QueryApplication(appIdToken);
                if (!string.IsNullOrWhiteSpace(application.AppName))
                {
                    SetApplicationToCache(application);
                }
            }

            if (string.IsNullOrWhiteSpace(application.AppName))
            {
                return AuthenticateResult.Fail($"Application with token {appIdToken} is not found.");
            }

            var serviceFullname = Context.Items["ServiceFullname"] as string;
            if (string.IsNullOrWhiteSpace(serviceFullname))
            {
                Context.Items["ServiceFullname"] = application.AppName;
            }

            var claims = new[] {
                new Claim("AppID", application.AppID),
                new Claim("AppName", application.AppName)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private async Task<Application> QueryApplication(string appId)
        {
            var queryRequest = new QueryRequest
            {
                TableName = Environment.GetEnvironmentVariable("APPLICATION_TABLE_NAME"),
                IndexName = Environment.GetEnvironmentVariable("APPLICATION_INDEX_NAME"),
                KeyConditionExpression = "appid = :v_appid",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    { ":v_appid", new AttributeValue { S = appId } }
                },
                ProjectionExpression = "appid, appname",
                ScanIndexForward = false
            };

            var result = await _dynamoDbClient.QueryAsync(queryRequest);
            if (result.Items.Count < 1)
            {
                return new Application();
            }

            var appItem = result.Items[0];
            return new Application()
            {
                AppID = appItem["appid"].S,
                AppName = appItem["appname"].S
            };
        }

        private Application GetApplicationFromCache(string appId)
        {
            if (_memoryCache.TryGetValue(appId, out Application cacheValue))
            {
                return cacheValue;
            }
            else
            {
                return new Application();
            }
        }

        private void SetApplicationToCache(Application application)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(AuthenticationConstants.MemoryCacheTTL));
            _memoryCache.Set(application.AppID, application, cacheEntryOptions);
        }
    }
}