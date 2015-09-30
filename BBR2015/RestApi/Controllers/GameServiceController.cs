using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Modell;
using Repository;

namespace RestApi.Controllers
{
    [EnableCors("*", "*", "*")]
    [RequireApiKey]
    public class GameServiceController : BaseController
    {
        // POST: api/GameService
        [HttpPost]
        [ResponseType(typeof(OkResult))]
        public IHttpActionResult Post([FromBody] RegistrerNyPost registrerNyPost)
        {
            try
            {
                if (registrerNyPost == null)
                {
                    return BadRequest("RegistrerNyPost cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var postRegistrering = GameServiceRepository.RegistrerNyPost(DeltakerId, LagId, registrerNyPost);
                if (postRegistrering == null)
                {
                    return Conflict();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }      
    }
}
