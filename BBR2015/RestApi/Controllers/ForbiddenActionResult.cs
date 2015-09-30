using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace RestApi.Controllers
{
    public class ForbiddenActionResult : IHttpActionResult
    {
        private readonly HttpRequestMessage _request;
        private readonly string _reason;
        public ForbiddenActionResult(HttpRequestMessage request, string reason)
        {
            _request = request;
            _reason = reason;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = _request.CreateResponse(HttpStatusCode.Forbidden, _reason);
            return Task.FromResult(response);
        }
    }
}