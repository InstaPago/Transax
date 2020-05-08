using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Reflection;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Caching;

namespace MyHelpers.Atributos
{
    public class CacheFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Gets or sets the cache duration in seconds. The default is 10 seconds.
        /// </summary>
        /// <value>The cache duration in seconds.</value>
        public int Duration
        {
            get;
            set;
        }

        public CacheFilterAttribute()
        {
            Duration = 10;
        }
                
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (Duration <= 0) return;

            HttpCachePolicyBase cache = filterContext.HttpContext.Response.Cache;
            TimeSpan cacheDuration = TimeSpan.FromSeconds(Duration);

            cache.SetCacheability(HttpCacheability.Public);
            cache.SetExpires(DateTime.Now.Add(cacheDuration));
            cache.SetMaxAge(cacheDuration);
            cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
        }
    }

    // Esta clase no me esta sirviendo con el EnableCompresion
    public class ActionOutputCacheAttribute : ActionFilterAttribute
    {
        // This hack is optional; I'll explain it later in the blog post
        private static MethodInfo _switchWriterMethod = typeof(HttpResponse).GetMethod("SwitchWriter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public ActionOutputCacheAttribute(int cacheDuration)
        {
            _cacheDuration = cacheDuration;
        }

        private int _cacheDuration;
        private TextWriter _originalWriter;
        private string _cacheKey;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _cacheKey = ComputeCacheKey(filterContext);
            string cachedOutput = (string)filterContext.HttpContext.Cache[_cacheKey];
            if (cachedOutput != null)
                filterContext.Result = new ContentResult { Content = cachedOutput };
            else
                _originalWriter = (TextWriter)_switchWriterMethod.Invoke(HttpContext.Current.Response, new object[] { new HtmlTextWriter(new StringWriter()) });
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (_originalWriter != null) // Must complete the caching
            {
                HtmlTextWriter cacheWriter = (HtmlTextWriter)_switchWriterMethod.Invoke(HttpContext.Current.Response, new object[] { _originalWriter });
                string textWritten = ((StringWriter)cacheWriter.InnerWriter).ToString();
                filterContext.HttpContext.Response.Write(textWritten);

                filterContext.HttpContext.Cache.Add(_cacheKey, textWritten, null, DateTime.Now.AddSeconds(_cacheDuration), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
            }
        }

        private string ComputeCacheKey(ActionExecutingContext filterContext)
        {
            var keyBuilder = new StringBuilder();
            foreach (var pair in filterContext.RouteData.Values)
                keyBuilder.AppendFormat("rd{0}_{1}_", pair.Key.GetHashCode(), pair.Value.GetHashCode());
            foreach (var pair in filterContext.ActionParameters)
                keyBuilder.AppendFormat("ap{0}_{1}_", pair.Key.GetHashCode(), pair.Value.GetHashCode());
            return keyBuilder.ToString();
        }
    }
}
