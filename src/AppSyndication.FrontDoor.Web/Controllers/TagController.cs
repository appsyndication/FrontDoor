using System;
using AppSyndication.BackendModel.IndexedData;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.FrontDoor.Web.Controllers
{
    //  [ResponseCache(Duration = 20)]
    [Route("[controller]")]
    public class TagController : Controller
    {
        private readonly ILogger _logger;
        private readonly ITagIndex _tagIndex;

        public TagController(ILoggerFactory loggerFactory, ITagIndex tagIndex)
        {
            _logger = loggerFactory.CreateLogger<TagController>();

            _tagIndex = tagIndex;
        }

        [HttpGet("{id}/{version?}/{os?}/{arch?}")]
        public IActionResult Display(string id, string version, string os, string arch, string format = null)
        {
            var negotiatedFormat = this.NegotiateFormat(format);

            if (negotiatedFormat == Format.Html)
            {
                return this.RedirectToRoute("DisplayApp", new { id, version, os, arch });
            }

            // TODO: use version, os, arch to make sure we're getting the right tag.
            var tagUri = _tagIndex.GetTagUri(id, negotiatedFormat == Format.Xml);

            if (tagUri == null)
            {
                _logger.LogWarning($"Could not find tag: {id}");

                return this.HttpNotFound();
            }

            return this.Redirect(tagUri);
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

        private enum Format
        {
            Html,
            Json,
            Xml,
        }
    }
}
