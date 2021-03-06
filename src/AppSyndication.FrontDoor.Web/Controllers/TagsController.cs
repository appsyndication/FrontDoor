using System;
using AppSyndication.BackendModel.IndexedData;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.FrontDoor.Web.Controllers
{
    //  [ResponseCache(Duration = 20)]
    [Route("[controller]")]
    public class TagsController : Controller
    {
        private const int PerPage = 30;
        private readonly ILogger _logger;
        private readonly ITagIndex _tagIndex;

        public TagsController(ILoggerFactory loggerFactory, ITagIndex tagIndex)
        {
            _logger = loggerFactory.CreateLogger<TagsController>();

            _tagIndex = tagIndex;
        }

        public IActionResult Index(string format = null, string q = null, int pg = 1)
        {
            var negotiatedFormat = this.NegotiateFormat(format);

            return this.IndexAsTag(negotiatedFormat, q, pg);
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
