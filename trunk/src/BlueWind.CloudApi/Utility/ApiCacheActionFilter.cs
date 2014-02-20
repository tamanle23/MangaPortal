using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.IO.IsolatedStorage;

namespace BlueWind.CloudApi.Utility
{
    public class ApiCacheActionFilterAttribute : ActionFilterAttribute
    {
        private int duration;
        private int clientDuration;
        private bool isOnlyForAnonymous;
        private string key;
        private object _lock = new object();
        public ApiCacheActionFilterAttribute(int timespan, int clientTimeSpan, bool anonymousOnly)
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
            cachecontrol.MaxAge = TimeSpan.FromSeconds(clientDuration);
            cachecontrol.MustRevalidate = true;
            return cachecontrol;
        }

        public override void OnActionExecuting(HttpActionContext context)
        {
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    if (context != null)
                    {
                        if (IsCacheable(context))
                        {
                            key = string.Join(":", new string[] { context.Request.RequestUri.PathAndQuery, });
                            if (WebApiCache.Contains(key))
                            {
                                var val = (HttpResponseMessage)WebApiCache.Get(key);
                                if (val != null)
                                {
                                    context.Response = context.Request.CreateResponse();
                                    context.Response.Content = val.Content;
                                    context.Response.Headers.CacheControl = GetClientCache();
                                    return;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (!(WebApiCache.Contains(key)))
            {
                var body = actionExecutedContext.Response.Content.ReadAsStringAsync().Result;
                WebApiCache.Add(key, actionExecutedContext.Response, DateTime.Now.AddSeconds(duration));
            }
            if (IsCacheable(actionExecutedContext.ActionContext))
                actionExecutedContext.ActionContext.Response.Headers.CacheControl = GetClientCache();
        }
    }
}