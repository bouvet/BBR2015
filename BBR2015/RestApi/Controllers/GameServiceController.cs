using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Repository;
using RestApi.Models;

namespace RestApi.Controllers
{
    [RequireApiKey]
    public class GameServiceController : BaseController
    {
        private GameServiceRepository _gameServiceRepository;

        public GameServiceController(GameServiceRepository gameServiceRepository)
        {
            _gameServiceRepository = gameServiceRepository;
        }
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

                _gameServiceRepository.RegistrerNyPost(DeltakerId, LagId, registrerNyPost.PostKode, registrerNyPost.BruktVåpen);
                
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }      
    }
}
