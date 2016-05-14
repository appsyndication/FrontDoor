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

        public IActionResult Index(string format = null, string q = null, int pg = 1)
        {
            var negotiatedFormat = this.NegotiateFormat(format);

            if (negotiatedFormat == Format.Html)
            {
                return this.IndexAsHtml(q, pg);
            }
            else
            {
                return this.IndexAsTag(negotiatedFormat, q, pg);
            }
        }

        [HttpGet("{id}/{version?}/{os?}/{arch?}")]
        public IActionResult Display(string id, string version, string os, string arch, string format = null)
        {
            var negotiatedFormat = this.NegotiateFormat(format);

            if (negotiatedFormat == Format.Html)
            {
                return this.DisplayAsHtml(id, version);
            }
            else
            {
                return this.DisplayAsTag(negotiatedFormat, id, version, os, arch);
            }
        }

        [Route("{id}/v{version}/download/{os?}/{arch?}")]
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

        private Format NegotiateFormat(string format)
        {
            Format result;

            if (!Enum.TryParse(format, true, out result))
            {
                foreach (var acceptType in this.Request.Headers["Accept"])
                {
                    switch (acceptType.ToLowerInvariant())
                    {
                        case "text/html":
                        case "application/xhtml":
                        case "application/xhtml+xml":
                            return Format.Html;

                        case "application/json":
                            return Format.Json;

                        case "text/xml":
                            return Format.Xml;
                    }
                }

                result = Format.Html;
            }

            return result;
        }

        private IActionResult IndexAsHtml(string query, int page)
        {
            TagResults results;

            var watch = new Stopwatch();
            watch.Start();

            if (String.IsNullOrWhiteSpace(query))
            {
                results = _tagIndex.GetTags(page, PerPage);
            }
            else
            {
                results = _tagIndex.SearchTags(query, page, PerPage);
            }

            watch.Stop();
            _logger.LogInformation("Processed index action in: {0}", watch.Elapsed);

            return this.View(results);
        }

        private IActionResult IndexAsTag(Format format, string query, int page)
        {
            var formatString = format.ToString().ToLowerInvariant();

            if (String.IsNullOrWhiteSpace(query))
            {
                return this.Redirect($"https://appsyndication.blob.core.windows.net/tags/sources/https-github-com-appsyndication-test-tree-master/index.{formatString}.swidtag");
            }

            var results = _tagIndex.SearchTags(query, page, PerPage);

            return this.TagResultsAction(results, formatString);
        }

        private IActionResult DisplayAsHtml(string id, string version)
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

        private IActionResult DisplayAsTag(Format format, string id, string version, string os, string arch)
        {
            var tagUri = _tagIndex.GetTagUri(id, format == Format.Xml);

            if (tagUri == null)
            {
                _logger.LogWarning($"Could not find tag: {id}");

                return this.HttpNotFound();
            }

            return this.Redirect(tagUri);
        }

        private IActionResult TagResultsAction(TagResults results, string format)
        {
            var content = "{ \"error\": \"Not supported yet.\" }";
            return this.Content(content, "application/swid-tag+json");
        }

        private enum Format
        {
            Html,
            Json,
            Xml,
        }
    }
}
