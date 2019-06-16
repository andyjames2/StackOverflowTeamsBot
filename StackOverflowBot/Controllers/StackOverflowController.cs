using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Controllers
{

    [ApiController]
    [Route("api/stackoverflow")]
    public class StackOverflowController : ControllerBase
    {

        private readonly IRepository<Subscription> _repository;
        private readonly IConfiguration _configuration;

        public StackOverflowController(IRepository<Subscription> repository, IConfiguration configuration)
        {
            this._repository = repository;
            this._configuration = configuration;
        }

        [HttpPost]
        [HttpGet]
        public async Task PostAsync()
        {
            var appId = this._configuration.GetValue<string>("MicrosoftAppId");
            var appPassword = this._configuration.GetValue<string>("MicrosoftAppPassword");

            foreach (var sub in this._repository.Get())
            {
                await sub.Send(appId, appPassword);
            }
        }

    }
}
