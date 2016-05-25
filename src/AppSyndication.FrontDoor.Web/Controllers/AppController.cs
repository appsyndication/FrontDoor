using System;
using System.Diagnostics;
using System.Linq;
using AppSyndication.BackendModel.IndexedData;
using AppSyndication.FrontDoor.Web.Models;
using AppSyndication.Web.Models;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.FrontDoor.Web.Controllers
{
    //  [ResponseCache(Duration = 20)]
    [Route("[controller]")]
    public class AppController : Controller
    {
        private readonly ILogger _logger;
        private readonly ITagIndex _tagIndex;

        public AppController(ILoggerFactory loggerFactory, ITagIndex tagIndex)
        {
            _logger = loggerFactory.CreateLogger<AppController>();

            _tagIndex = tagIndex;
        }

        [HttpGet("{id}/{version?}/{os?}/{arch?}", Name = "DisplayApp")]
        public IActionResult Display(string id, string version, string os, string arch, string format = null)
        {
            var watch = new Stopwatch();
            watch.Start();

            _logger.LogInformation($"Display action for id: {id}, version: {version}");

            var tag = _tagIndex.GetTagByAliasOrId(id);

            if (tag == null)
            {
                _logger.LogWarning($"Could not find tag: {id}");
                return this.HttpNotFound();
            }

            var histories = _tagIndex.GetTagHistory(tag.Id).ToList();

            // If the requested version is not the latest tag, dig through history to
            // try to find the match.
            if (!String.IsNullOrEmpty(version) && !tag.Version.Equals(version, StringComparison.OrdinalIgnoreCase))
            {
                var tagHistory = histories.FirstOrDefault(h => h.Version.Equals(version, StringComparison.OrdinalIgnoreCase));

                if (tagHistory == null)
                {
                    _logger.LogWarning($"Could not find version of tag: {id} v{version}");
                    return this.HttpNotFound();
                }

                // TODO: load the versioned tag from the blob in storage.
                // tagHistory.BlobUri
            }

            // TODO: process os and arch

            var tagWithHistory = new TagWithHistory(tag, histories);

            watch.Stop();
            _logger.LogInformation($"Processed display action in: {watch.Elapsed}");

            this.ViewBag.RequestAliasOrId = id;
            this.ViewBag.RequestVersion = version;

            return this.View(tagWithHistory);
        }

        [Route("{id}/v{version}/download/{os?}/{arch?}", Name = "DownloadApp")]
        public IActionResult Download(string id, string version, string os, string arch)
        {
            var redirects = _tagIndex.GetTagDownloadRedirects(id, version).ToList();

            if (!redirects.Any())
            {
                _logger.LogWarning($"Could not find download redirect for tag: {id} v{version}");

                return this.HttpNotFound();
            }

            var ua = this.Request.Headers["User-Agent"];
            var userAgent = new UserAgent(ua);
            var architecture = userAgent.Architecture;
            var platform = userAgent.Platform;

            if (!String.IsNullOrEmpty(os))
            {
                Enum.TryParse(os, true, out platform);
            }

            if (!String.IsNullOrEmpty(arch))
            {
                Enum.TryParse(arch, true, out architecture);
            }

            // TODO: actually filter the list of redirects based on user agent instead
            //       of simply taking the first redirect we find.
            var redirect = redirects.First();

            return this.Redirect(redirect.Uri);

            //  return $"> id: {id} v{version} for OS: {platform} on {architecture}\n> user-agent: {ua}";
        }
    }
}
