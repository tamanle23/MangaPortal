using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.IO.IsolatedStorage;
using System.Web;


namespace MangaPortal.Filters
{
    public class ApiCacheAttribute : ActionFilterAttribute
    {
        private int duration;
        private int clientDuration;
        private bool isOnlyForAnonymous;
        private object _lock = new object();

        public ApiCacheAttribute(int timespan, int clientTimeSpan, bool anonymousOnly)
        {
            duration = timespan;
            clientDuration = clientTimeSpan;
            isOnlyForAnonymous = anonymousOnly;
        }

        private static ObjectCache WebApiCache
        {
            get
            {
                return MemoryCache.Default;
            }
        }

        private bool IsCacheable(HttpActionContext context)
        {
            if (duration > 0 && clientDuration > 0)
            {
                if (isOnlyForAnonymous)
                    if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                        return false;
                if (context.Request.Method == HttpMethod.Get) return true;
            }
            else
            {
                throw new InvalidOperationException("Wrong Arguments");
            }
            return false;
        }

        private CacheControlHeaderValue GetClientCache()
        {
            var cachecontrol = new CacheControlHeaderValue();
            cachecontrol.MaxAge = TimeSpan.FromMilliseconds(clientDuration);
            cachecontrol.MustRevalidate = true;
            return cachecontrol;
        }

        public override void OnActionExecuting(HttpActionContext context)
        {
            var cacheKey = string.Join(":", context.Request.Headers.Accept.Select(n => n.MediaType.ToString()).Concat(new string[] { context.Request.RequestUri.PathAndQuery }));
            context.ActionArguments["cacheKey"] = cacheKey;
#if !DEBUG

            if (context != null)
            {
                if (IsCacheable(context))
                {
                    if (WebApiCache.Contains(cacheKey))
                    {
                        var cacheContent = (string)WebApiCache.Get(cacheKey);
                        var cacheContentType = (MediaTypeHeaderValue)WebApiCache.Get(cacheKey+":ct");
                        if (cacheContent != null)
                        {
                            context.Response = context.Request.CreateResponse();
                            context.Response.Content = new StringContent(cacheContent);
                            context.Response.Content.Headers.ContentType = cacheContentType;
                            context.Response.Headers.CacheControl = GetClientCache();
                            return;
                        }
                    }

                  
                }
            }
#endif
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            var cacheKey = (string)actionExecutedContext.ActionContext.ActionArguments["cacheKey"];
            if (!(WebApiCache.Contains(cacheKey)))
            {
                var body = actionExecutedContext.Response.Content.ReadAsStringAsync().Result;
                var contentType=actionExecutedContext.Response.Content.Headers.ContentType;
                WebApiCache.Add(cacheKey, body, DateTime.Now.AddMilliseconds(duration));
                WebApiCache.Add(cacheKey + ":ct", contentType, DateTime.Now.AddMilliseconds(duration));
            }
            if (IsCacheable(actionExecutedContext.ActionContext))
                actionExecutedContext.ActionContext.Response.Headers.CacheControl = GetClientCache();
            
        }
    }
}