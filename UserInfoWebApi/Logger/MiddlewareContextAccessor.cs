using Microsoft.AspNetCore.Http;

namespace UserInfoWebApi.Logger
{
    public class MiddlewareContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        public string GetTraceId()
        {
            return httpContextAccessor.HttpContext?.Items["TraceId"] as string ?? string.Empty;
        }

        public string GetServiceFullname()
        {
            return httpContextAccessor.HttpContext?.Items["ServiceFullname"] as string ?? string.Empty;
        }

        public string GetSdkVersion()
        {
            return httpContextAccessor.HttpContext?.Items["SdkVersion"] as string ?? string.Empty;
        }

        public string GetPath()
        {
            return httpContextAccessor.HttpContext?.Items["Path"] as string ?? string.Empty;
        }
    }
}