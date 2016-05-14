using System.Diagnostics;
using AppSyndication.BackendModel.IndexedData;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.FrontDoor.Web.Controllers
{
    //  [ResponseCache(Duration = 20)]
    [Route("{channelId:regex(-.*)}")]
    public class ChannelController : Controller
    {
        private ILogger _logger;

        private ITagIndex _tagContext;

        public ChannelController(ILoggerFactory loggerFactory, ITagIndex tagContext)
        {
            _logger = loggerFactory.CreateLogger<ChannelController>();

            _tagContext = tagContext;
        }

        public IActionResult Index(string channelId)
        {
            Trace.WriteLine($"Channel {channelId} home page displayed.");

            return this.View();
        }

        [Route("edit")]
        public string Edit(string channelId)
        {
            Trace.WriteLine($"Edit channel {channelId} home page displayed.");

            return $"Edit channel {channelId} page displayed.";
        }

    }
}
