using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Database;

namespace RestApi.Filters
{
    public class ThrottleAttribute : ActionFilterAttribute
    {
        private static ConcurrentDictionary<string, DateTime> _lastRequest = new ConcurrentDictionary<string, DateTime>();
        private static int _minsteTidMellom = 1;
        static ThrottleAttribute()
        {
            Reload();
        }
        public override void OnActionExecuting(HttpActionContext context)
        {
            var lagKode = GetHeaderValue(context, Constants.Headers.HTTPHEADER_LAGKODE);
            var deltakerKode = GetHeaderValue(context, Constants.Headers.HTTPHEADER_DELTAKERKODE);

            var key = LagKey(lagKode, deltakerKode);

            if (_lastRequest.ContainsKey(key))
            {
                var current = TimeService.Now;

                if (current < _lastRequest[key].AddMilliseconds(_minsteTidMellom))
                {
                    var message = "Du maser. Minste intervall mellom requests i ms: " + _minsteTidMellom;
                    context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                }
                _lastRequest[key] = current;
            }
        }

        private string LagKey(string lagKode, string deltakerKode)
        {
            return string.Format("{0}¤¤¤{1}", lagKode, deltakerKode);
        }
        private string GetHeaderValue(HttpActionContext context, string key)
        {
            var header = context.Request.Headers.SingleOrDefault(x => x.Key == key);

            return header.Value != null ? header.Value.FirstOrDefault() : null;
        }

        public static void Reload()
        {
            _minsteTidMellom = new OverridableSettings().MinsteTidMellomRequestsIMs;
            _lastRequest = new ConcurrentDictionary<string, DateTime>();
        }
    }
}