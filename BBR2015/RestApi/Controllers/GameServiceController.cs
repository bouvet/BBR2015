using System.Collections.Generic;
using System.Web.Http;

namespace RestApi.Controllers
{
    public class GameServiceController : ApiController
    {
        // GET: api/GameService
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/GameService/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/GameService
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/GameService/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/GameService/5
        public void Delete(int id)
        {
        }
    }
}
