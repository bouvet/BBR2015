using System.Web.Http;

namespace RestApi.Controllers
{
    public class BaseController : ApiController
    {
        public string LagId
        {
            get
            {
                return Request.Properties["LagId"].ToString();
            }
        }
        public string DeltakerId
        {
            get
            {
                return Request.Properties["DeltakerId"].ToString();
            }
        }
    }
}
