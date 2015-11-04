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
            var lagKode = GetHeaderValue(context, HTTPHEADER_LAGKODE);
            var deltakerKode = GetHeaderValue(context, HTTPHEADER_DELTAKERKODE);

            if (string.IsNullOrEmpty(lagKode) || string.IsNullOrEmpty(deltakerKode))
            {
                var message = string.Format("Mangler en eller flere påkrevde HTTP Headere: '{0}', '{1}'", HTTPHEADER_LAGKODE, HTTPHEADER_DELTAKERKODE);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
                return;
            }

            var tilgangsKontroll = ServiceLocator.Current.Resolve<TilgangsKontroll>();

            var resultat = tilgangsKontroll.SjekkTilgang(lagKode, deltakerKode);

            if (resultat == null)
            {
                var message = string.Format("Ugyldige verdier i HTTP Headere: '{0}', '{1}'. Sjekk lagoppstillingen.", HTTPHEADER_LAGKODE, HTTPHEADER_DELTAKERKODE);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message);
                return;
            }

            context.Request.Properties[REQUESTPROPERTY_LAGID] = resultat.LagId;
            context.Request.Properties[REQUESTPROPERTY_DELTAKERID] = resultat.DeltakerId;
        }        
        private string GetHeaderValue(HttpActionContext context, string key)
        {
            var header = context.Request.Headers.SingleOrDefault(x => x.Key == key);

            return header.Value != null ? header.Value.FirstOrDefault() : null;
        }
      
    }
}