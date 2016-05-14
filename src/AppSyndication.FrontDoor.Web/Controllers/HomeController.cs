using System.Diagnostics;
using AppSyndication.BackendModel.IndexedData;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.FrontDoor.Web.Controllers
{
    //  [ResponseCache(Duration = 20)]
    [Route("")]
    public class HomeController : Controller
    {
        private ILogger _logger;

        private readonly ITagIndex _tagContext;

        public HomeController(ILoggerFactory loggerFactory, ITagIndex tagContext)
        {
            _logger = loggerFactory.CreateLogger<HomeController>();

            _tagContext = tagContext;
        }

        public IActionResult Index()
        {
            Trace.WriteLine("Home page displayed.");

            return this.View();
        }

        [Route("[action]")]
        public IActionResult About()
        {
            return this.View();
        }

        [Route("[action]")]
        public IActionResult Remove([FromHeader(Name = "User-Agent")] string userAgent)
        {
            _tagContext.Clear();

            return this.View(userAgent as object);
        }

        //          public IActionResult Error()
        //          {
        //              return View("~/Views/Shared/Error.cshtml");
        //          }

        [Route("[action]")]
        public IActionResult StatusCodePage()
        {
            return this.View("~/Views/Shared/StatusCodePage.cshtml");
        }
    }
}
