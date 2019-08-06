using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackOverflowBot.Links;
using StackOverflowBot.Repositories;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Controllers
{

    public class StackOverflowController : Controller
    {

        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly IRepository<Link> _linkRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public StackOverflowController(IRepository<Subscription> subscriptionRepository, IRepository<Link> linkRepository, IConfiguration configuration, HttpClient httpClient)
        {
            this._subscriptionRepository = subscriptionRepository;
            this._linkRepository = linkRepository;
            this._configuration = configuration;
            this._httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Link([FromRoute] string linkKey)
        {
            var link = this._linkRepository.Get().FirstOrDefault(r => r.LinkKey == linkKey);
            if (link == null || link.State != LinkState.SettingUp)
                return this.Unauthorized();
            var clientId = this._configuration.GetValue<string>("StackOverflowClientId");
            var rootUrl = this._configuration.GetValue<string>("RootUrlForLinks");
            if (!rootUrl.EndsWith("/")) rootUrl += "/";
            rootUrl = Uri.EscapeDataString(rootUrl);
            var teamId = Uri.EscapeDataString(link.TeamId);
            var url = $"https://stackoverflow.com/oauth?client_id={clientId}&scope=no_expiry,access_team|stackoverflow.com/c/{teamId}&redirect_uri={rootUrl}so/authorize/{linkKey}";
            return View(model: url);
        }

        [HttpGet]
        public async Task<IActionResult> Authorize([FromRoute] string linkKey, [FromQuery] string code)
        {
            var link = this._linkRepository.Get().FirstOrDefault(r => r.LinkKey == linkKey);
            if (link == null || link.State != LinkState.SettingUp)
                return this.Unauthorized();

            var clientId = this._configuration.GetValue<string>("StackOverflowClientId");
            var clientSecret = this._configuration.GetValue<string>("StackOverflowClientSecret");
            var rootUrl = this._configuration.GetValue<string>("RootUrlForLinks");
            if (!rootUrl.EndsWith("/")) rootUrl += "/";
            rootUrl = Uri.EscapeDataString(rootUrl);
            var redirectUrl = rootUrl += "so/authorize/" + linkKey;

            await link.GetAccessToken(this._httpClient, clientId, clientSecret, code, redirectUrl);
            this._linkRepository.SaveOrUpdate(link);

            var appId = this._configuration.GetValue<string>("MicrosoftAppId");
            var appPassword = this._configuration.GetValue<string>("MicrosoftAppPassword");

            await link.Target.SendConfirmation(appId, appPassword);

            return this.View();
        }

    }
}
