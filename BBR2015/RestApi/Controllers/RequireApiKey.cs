using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Repository;

namespace RestApi.Controllers
{
    public class RequireApiKey : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
        {
            var validLagId = EnsureApiKey(context, "LagId");
            if (!validLagId) return;
            EnsureApiKey(context, "DeltakerId");
        }

        private static bool EnsureApiKey(HttpActionContext context, string key)
        {
            var header = context.Request.Headers.SingleOrDefault(x => x.Key == key);
            var keyId = string.Empty;

            var valid = header.Value != null;
            if (valid)
            {
                keyId = header.Value.FirstOrDefault();
                switch (key)
                { case "LagId" :
                        valid = AdminRepository.GyldigLag(keyId);
                        break;
                    case "DeltakerId":
                        var lagId = context.Request.Properties["LagId"].ToString();
                        var lag = AdminRepository.FinnLag(lagId);
                        valid = lag != null && lag.GyldigDeltaker(keyId);
                        break;
                }
                context.Request.Properties[key] = keyId;
            }

            if (!valid)
            {
                var message = string.Format("Invalid Authorization Key ({0}) : {1}", key, keyId);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
            }
            return valid;
        }
    }
}