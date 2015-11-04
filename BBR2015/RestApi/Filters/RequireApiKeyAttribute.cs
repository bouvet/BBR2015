using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Database;
using Repository;
using RestApi.Infrastructure;

namespace RestApi.Filters
{
    public class RequireApiKeyAttribute : ActionFilterAttribute
    {
       
        public override void OnActionExecuting(HttpActionContext context)
        {
            var lagKode = GetHeaderValue(context, Constants.Headers.HTTPHEADER_LAGKODE);
            var deltakerKode = GetHeaderValue(context, Constants.Headers.HTTPHEADER_DELTAKERKODE);

            if (string.IsNullOrEmpty(lagKode) || string.IsNullOrEmpty(deltakerKode))
            {
                var message = string.Format("Mangler en eller flere påkrevde HTTP Headere: '{0}', '{1}'", Constants.Headers.HTTPHEADER_LAGKODE, Constants.Headers.HTTPHEADER_DELTAKERKODE);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
                return;
            }

            var tilgangsKontroll = ServiceLocator.Current.Resolve<TilgangsKontroll>();

            var resultat = tilgangsKontroll.SjekkTilgang(lagKode, deltakerKode);

            if (resultat == null)
            {
                var message = string.Format("Ugyldige verdier i HTTP Headere: '{0}', '{1}'. Sjekk lagoppstillingen.", Constants.Headers.HTTPHEADER_LAGKODE, Constants.Headers.HTTPHEADER_DELTAKERKODE);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
                return;
            }

            context.Request.Properties[Constants.Headers.REQUESTPROPERTY_LAGID] = resultat.LagId;
            context.Request.Properties[Constants.Headers.REQUESTPROPERTY_DELTAKERID] = resultat.DeltakerId;
        }        
        private string GetHeaderValue(HttpActionContext context, string key)
        {
            var header = context.Request.Headers.SingleOrDefault(x => x.Key == key);

            return header.Value != null ? header.Value.FirstOrDefault() : null;
        }
      
    }
}