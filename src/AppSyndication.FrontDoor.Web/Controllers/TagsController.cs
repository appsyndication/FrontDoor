using System;
using System.Linq;
using System.Threading.Tasks;
using AppSyndication.BackendModel.IndexedData;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc;
using AppSyndication.Web.Models;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace AppSyndication.Web.Controllers
{
    //  [ResponseCache(Duration = 20)]
    [Route("[controller]")]
    public class TagsController : Controller
    {
        private const int PerPage = 3000;

        private readonly ILogger _logger;
        private readonly ITagIndex _tagIndex;

        public TagsController(ILoggerFactory loggerFactory, ITagIndex tagIndex)
        {
            _logger = loggerFactory.CreateLogger<TagsController>();

            _tagIndex = tagIndex;
        }

        [Route("{id?}/{version?}", Name="Tag")]
        public IActionResult Tag([FromHeader(Name="Accept")] string accept, string id, string version, string format = null, string q = null, int pg = 1)
        {
            _logger.LogInformation($"tag, accepts: {accept}");

            // If format is explicitly XML use that, otherwise default to JSON.
            format = "xml".Equals(format, StringComparison.OrdinalIgnoreCase) ? "xml" : "json";

            if (!String.IsNullOrEmpty(id))
            {
                return this.TagAction(id, format);
            }

            if (String.IsNullOrWhiteSpace(q))
            {
                return this.Redirect($"https://appsyndication.blob.core.windows.net/tags/sources/https-github-com-appsyndication-test-tree-master/index.{format}.swidtag");
            }

            var results = _tagIndex.SearchTags(q, pg, PerPage);

            return this.TagResultsAction(results, format);
        }

        [Route("{id}/v{version}/downloads/{os?}/{arch?}", Name = "DownloadTag")]
        public IActionResult DownloadTag([FromHeader(Name="User-Agent")] string ua, string id, string version, string os, string arch)
        {
            var redirects = _tagIndex.GetTagDownloadRedirects(id, version);

            if (!redirects.Any())
            {
                _logger.LogWarning($"Could not find download redirect for tag: {id} v{version}");
                return this.HttpNotFound();
            }

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

            var connectionFeature = this.HttpContext.Features.Get<IHttpConnectionFeature>();

            var ip = connectionFeature?.RemoteIpAddress.ToString() ?? String.Empty;

            //await _tagIndex.IncrementDownloadRedirectCount(redirect.Key, ip);

            return this.Redirect(redirect.Uri);

            //  return $"> id: {id} v{version} for OS: {platform} on {architecture}\n> user-agent: {ua}";
        }

        private IActionResult TagAction(string id, string format)
        {
            var tagUri = _tagIndex.GetTagUri(id, format == "xml");

            if (tagUri == null)
            {
                _logger.LogWarning($"Could not find tag: {id}");

                return this.HttpNotFound();
            }

            return Redirect(tagUri);
        }

        private IActionResult TagResultsAction(TagResults results, string format)
        {
            var content = "{ \"error\": \"Not supported yet.\" }";
            return this.Content(content, "application/swid-tag+json");
        }
   }
}
