using Microsoft.Bot.Schema;

namespace StackOverflowBot.Registrations
{
    public class RegistrationConfirmationTarget
    {

        public string ServiceUrl { get; set; }
        public ChannelAccount Bot { get; set; }
        public string Target { get; set; }
        public string PlatformId { get; set; }

    }
}