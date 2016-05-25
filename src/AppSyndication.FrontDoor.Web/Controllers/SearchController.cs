using System.Diagnostics;
using AppSyndication.BackendModel.IndexedData;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.FrontDoor.Web.Controllers
{
    public class SearchController : Controller
    {
        private const int PerPage = 30;
        private readonly ILogger _logger;
        private readonly ITagIndex _tagIndex;

        public SearchController(ILoggerFactory loggerFactory, ITagIndex tagIndex)
        {
            _logger = loggerFactory.CreateLogger<SearchController>();

            _tagIndex = tagIndex;
        }

        [HttpGet("search", Name = "Search")]
        public IActionResult Index(string q = null, int pg = 1)
        {
            var watch = new Stopwatch();
            watch.Start();

            var results = _tagIndex.SearchTags(q, pg, PerPage);

            watch.Stop();
            _logger.LogInformation("Processed search query in: {0}", watch.Elapsed);

            return this.View(results);
        }
    }
}
