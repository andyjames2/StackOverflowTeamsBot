using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using StackOverflowBot.Registrations;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Controllers
{

    public class StackOverflowController : Controller
    {

        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly IRepository<Registration> _registrationRepository;
        private readonly IConfiguration _configuration;

        public StackOverflowController(IRepository<Subscription> subscriptionRepository, IRepository<Registration> registrationRepository, IConfiguration configuration)
        {
            this._subscriptionRepository = subscriptionRepository;
            this._registrationRepository = registrationRepository;
            this._configuration = configuration;
        }

        [HttpGet]
        public async Task Ping()
        {
            var appId = this._configuration.GetValue<string>("MicrosoftAppId");
            var appPassword = this._configuration.GetValue<string>("MicrosoftAppPassword");

            foreach (var sub in this._subscriptionRepository.Get())
                await sub.Send(appId, appPassword);
        }

        [HttpGet]
        public async Task<IActionResult> Register([FromRoute] string registrationKey)
        {
            var registration = this._registrationRepository.Get().FirstOrDefault(r => r.RegistrationKey == registrationKey);
            if (registration == null)
                return this.Unauthorized();
            var clientId = this._configuration.GetValue<string>("StackOverflowClientId");
            var rootUrl = this._configuration.GetValue<string>("RootUrl");
            if (!rootUrl.EndsWith("/")) rootUrl += "/";
            var url = $"https://stackoverflow.com/oauth?client_id={clientId}&scope=access_team|stackoverflow.com/c/{registration.TeamId}&redirect_uri={rootUrl}so/authorize/{registrationKey}";
            return View(model: url);
        }

        [HttpGet]
        public async Task<IActionResult> Authorize([FromRoute] string registrationKey)
        {
            var registration = this._registrationRepository.Get().FirstOrDefault(r => r.RegistrationKey == registrationKey);
            if (registration == null)
                return this.Unauthorized();

            var appId = this._configuration.GetValue<string>("MicrosoftAppId");
            var appPassword = this._configuration.GetValue<string>("MicrosoftAppPassword");

            var connector = new ConnectorClient(new Uri(registration.ConfirmationTarget.ServiceUrl), new MicrosoftAppCredentials(appId, appPassword));
            var message = Activity.CreateMessageActivity();
            message.ChannelId = registration.ConfirmationTarget.PlatformId;
            message.Conversation = new ConversationAccount(id: registration.ConfirmationTarget.Target);
            message.From = registration.ConfirmationTarget.Bot;
            message.Text = "Excellent! Your Stack Overflow team has been registered, get subscribing!";
            message.Locale = "en-GB";
            await connector.Conversations.SendToConversationAsync((Activity)message);

            return this.View();
        }


    }
}
