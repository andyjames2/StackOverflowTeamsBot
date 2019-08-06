using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace StackOverflowBot.Links
{
    public class LinkTarget
    {

        public string ServiceUrl { get; set; }
        public ChannelAccount Bot { get; set; }
        public string Id { get; set; }
        public string PlatformId { get; set; }

        public async Task SendConfirmation(string appId, string appPassword)
        {
            var connector = new ConnectorClient(new Uri(this.ServiceUrl), new MicrosoftAppCredentials(appId, appPassword));
            var message = Activity.CreateMessageActivity();
            message.ChannelId = this.PlatformId;
            message.Conversation = new ConversationAccount(id: this.Id);
            message.From = this.Bot;
            message.Text = "Excellent! Your Stack Overflow team has been linked, get subscribing!";
            message.Locale = "en-GB";
            await connector.Conversations.SendToConversationAsync((Activity) message);
        }

    }
}