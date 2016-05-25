using System;
using System.Diagnostics;
using System.Linq;
using AppSyndication.BackendModel.IndexedData;
using AppSyndication.FrontDoor.Web.Models;
using AppSyndication.Web.Models;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.FrontDoor.Web.Controllers
{
    //  [ResponseCache(Duration = 20)]
    [Route("[controller]")]
    public class AppsController : Controller
    {
        private const int PerPage = 30;
        private readonly ILogger _logger;
        private readonly ITagIndex _tagIndex;

        public AppsController(ILoggerFactory loggerFactory, ITagIndex tagIndex)
        {
            _logger = loggerFactory.CreateLogger<AppsController>();

            _tagIndex = tagIndex;
        }

        public IActionResult Index(int pg = 1)
        {
            var results = _tagIndex.GetTags(pg, PerPage);

            return this.View(results);
        }
    }
}
