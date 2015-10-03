using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Repository;
using RestApi.Infrastructure;

namespace RestApi.Controllers
{
    public class RequireApiKey : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
        {
            if (EnsureApiKey(context, "LagId"))
                EnsureApiKey(context, "Deltaker1");
        }

        private static bool EnsureApiKey(HttpActionContext context, string key)
        {
            var header = context.Request.Headers.SingleOrDefault(x => x.Key == key);
            var keyId = string.Empty;

            var valid = header.Value != null;
            if (valid)
            {
                var adminRepository = ServiceLocator.Current.Resolve<AdminRepository>();

                keyId = header.Value.FirstOrDefault();
                switch (key)
                {
                    case "LagId":
                        valid = adminRepository.GyldigLag(keyId);
                        break;
                    case "Deltaker1":
                        var lagId = context.Request.Properties["LagId"].ToString();
                        var lag = adminRepository.FinnLag(lagId);
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