using Microsoft.AspNet.Mvc;

namespace AppSyndication.FrontDoor.Web.Controllers
{
    //  [ResponseCache(Duration = 20)]
    [Route("howto")]
    public class HowToController : Controller
    {
        [Route("[action]")]
        public IActionResult OneGet()
        {
            return this.View();
        }
    }
}
