using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Repository;
using RestApi.Filters;
using RestApi.Models;

namespace RestApi.Controllers
{
    [RequireApiKey]
    public class GameServiceController : BaseController
    {
        private GameService _gameService;

        public GameServiceController(GameService gameService)
        {
            _gameService = gameService;
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

                _gameService.RegistrerNyPost(DeltakerId, LagId, registrerNyPost.PostKode, registrerNyPost.BruktVåpen);
                
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }      
    }
}
