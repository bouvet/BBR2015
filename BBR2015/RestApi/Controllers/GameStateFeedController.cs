using System.Web.Http.Cors;

namespace RestApi.Controllers
{
    public class GameStateFeedController : BaseController
    {
        // GET: api/GameStateFeed
        [EnableCors("*", "*", "*")]
        [RequireApiKey]
        public object Get()
        {
            /* FOR ETT LAG
             *Poster
             *m koorinat
             *m poeng
             *stemplet
             *
             * Sum poeng
             * Våpenbeholdning
             */
            return new { Lag = LagId };
        }
    }
}
