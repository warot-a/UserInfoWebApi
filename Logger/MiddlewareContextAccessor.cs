using Microsoft.AspNetCore.Http;

namespace UserInfoWebApi.Logger
{
    public class MiddlewareContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MiddlewareContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTraceId()
        {
            return _httpContextAccessor.HttpContext?.Items["TraceId"] as string ?? string.Empty;
        }
        public string GetServiceFullname()
        {
            return _httpContextAccessor.HttpContext?.Items["ServiceFullname"] as string ?? string.Empty;
        }
        public string GetSdkVersion()
        {
            return _httpContextAccessor.HttpContext?.Items["SdkVersion"] as string ?? string.Empty;
        }
        public string GetPath()
        {
            return _httpContextAccessor.HttpContext?.Items["Path"] as string ?? string.Empty;
        }
    }
}
