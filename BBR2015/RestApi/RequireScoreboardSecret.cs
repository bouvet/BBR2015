using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Database;
using RestApi.Infrastructure;

namespace RestApi
{
    public class RequireScoreboardSecret : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
        {
            var header = context.Request.Headers.SingleOrDefault(x => x.Key == "ScoreboardSecret");

            if (header.Value == null)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Authorization Key");
                return;
            }

            var appSettings = ServiceLocator.Current.Resolve<CascadingAppSettings>();

            var value = header.Value.FirstOrDefault();

            if (value != appSettings.ScoreboardSecret)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Authorization Key");                
            }
        }
    }
}