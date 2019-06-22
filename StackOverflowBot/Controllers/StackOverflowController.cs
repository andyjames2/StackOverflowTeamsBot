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
        private readonly IRepository<Link> _registrationRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public StackOverflowController(IRepository<Subscription> subscriptionRepository, IRepository<Link> registrationRepository, IConfiguration configuration, HttpClient httpClient)
        {
            this._subscriptionRepository = subscriptionRepository;
            this._registrationRepository = registrationRepository;
            this._configuration = configuration;
            this._httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Link([FromRoute] string registrationKey)
        {
            var registration = this._registrationRepository.Get().FirstOrDefault(r => r.RegistrationKey == registrationKey);
            if (registration == null || registration.State != RegistrationState.SettingUp)
                return this.Unauthorized();
            var clientId = this._configuration.GetValue<string>("StackOverflowClientId");
            var rootUrl = this._configuration.GetValue<string>("RootUrlForLinks");
            if (!rootUrl.EndsWith("/")) rootUrl += "/";
            rootUrl = Uri.EscapeDataString(rootUrl);
            var teamId = Uri.EscapeDataString(registration.TeamId);
            var url = $"https://stackoverflow.com/oauth?client_id={clientId}&scope=access_team|stackoverflow.com/c/{teamId}&redirect_uri={rootUrl}so/authorize/{registrationKey}";
            return View(model: url);
        }

        [HttpGet]
        public async Task<IActionResult> Authorize([FromRoute] string registrationKey, [FromQuery] string code)
        {
            var registration = this._registrationRepository.Get().FirstOrDefault(r => r.RegistrationKey == registrationKey);
            if (registration == null || registration.State != RegistrationState.SettingUp)
                return this.Unauthorized();

            var clientId = this._configuration.GetValue<string>("StackOverflowClientId");
            var clientSecret = this._configuration.GetValue<string>("StackOverflowClientSecret");
            var rootUrl = this._configuration.GetValue<string>("RootUrlForLinks");
            if (!rootUrl.EndsWith("/")) rootUrl += "/";
            var redirectUrl = rootUrl += "so/authorize/" + registrationKey;

            await registration.GetAccessToken(this._httpClient, clientId, clientSecret, code, redirectUrl);
            this._registrationRepository.SaveOrUpdate(registration);

            var appId = this._configuration.GetValue<string>("MicrosoftAppId");
            var appPassword = this._configuration.GetValue<string>("MicrosoftAppPassword");

            await registration.Target.SendConfirmation(appId, appPassword);

            return this.View();
        }

    }
}
