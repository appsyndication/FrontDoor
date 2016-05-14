using System.Diagnostics;
using AppSyndication.BackendModel.IndexedData;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.Web.Controllers
{
    [Route("{userId:regex(@.*)}")]
    public class UserController : Controller
    {
        private ILogger _logger;

        private ITagIndex _tagContext;

        public UserController(ILoggerFactory loggerFactory, ITagIndex tagContext)
        {
            _logger = loggerFactory.CreateLogger<UserController>();

            _tagContext = tagContext;
        }

        public IActionResult Index(string userId)
        {
            userId = userId.Substring(1);

            Trace.WriteLine($"User {userId} home page displayed.");

            return View(new M() { UserId = userId });
        }

        [Route("apps")]
        public string Apps(string userId, string q = null, int pg = 1)
        {
            return $"Apps for {userId} query {q} on page {pg}";
        }

        [Route("apps/{id}/{version?}")]
        public string App(string userId, string id, string version)
        {
            return $"App {id} v{version} for {userId}";
        }

        [Route("tags")]
        public string Tags(string userId)
        {
            return $"Tags for {userId}";
        }
    }

    public class M
    {
        public string UserId { get; set; }
    }
}
