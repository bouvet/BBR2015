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
        private const string HTTPHEADER_LAGKODE = "LagId";
        private const string REQUESTPROPERTY_LAGID = "LagId";

        private const string HTTPHEADER_DELTAKERKODE = "DeltakerId";
        private const string REQUESTPROPERTY_DELTAKERID = "DeltakerId";
        public override void OnActionExecuting(HttpActionContext context)
        {
            if (ValiderLag(context))
                ValiderDeltaker(context);
        }
        private bool ValiderLag(HttpActionContext context)
        {
            var headerValue = GetHeaderValue(context, HTTPHEADER_LAGKODE);
           
            if (!string.IsNullOrEmpty(headerValue))
            {
                var adminRepository = ServiceLocator.Current.Resolve<AdminRepository>();

                var lagId = adminRepository.FinnLagIdFraKode(headerValue);

                if (!string.IsNullOrEmpty(lagId))
                {
                    context.Request.Properties[REQUESTPROPERTY_LAGID] = lagId;
                    return true;
                }
            }

            var message = string.Format("Invalid Authorization Key ({0}) : {1}", HTTPHEADER_LAGKODE, headerValue);
            context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
            return false;
        }

        private void ValiderDeltaker(HttpActionContext context)
        {
            var headerValue = GetHeaderValue(context, HTTPHEADER_DELTAKERKODE);

            if (!string.IsNullOrEmpty(headerValue))
            {
                var adminRepository = ServiceLocator.Current.Resolve<AdminRepository>();

                var lagId = context.Request.Properties[REQUESTPROPERTY_LAGID].ToString();
                var deltakerId = adminRepository.SlåOppDeltakerFraKode(lagId, headerValue);

                if (!string.IsNullOrEmpty(deltakerId))
                {
                    context.Request.Properties[REQUESTPROPERTY_DELTAKERID] = deltakerId;
                    return;
                }
            }

            var message = string.Format("Invalid Authorization Key ({0}) : {1}", HTTPHEADER_DELTAKERKODE, headerValue);
            context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
        }

        private string GetHeaderValue(HttpActionContext context, string key)
        {
            var header = context.Request.Headers.SingleOrDefault(x => x.Key == key);

            return header.Value?.FirstOrDefault();
        }
      
    }
}