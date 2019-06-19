using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using StackOverflowBot.Querying.Model;

namespace StackOverflowBot.Subscriptions
{
    public class Subscription : IEquatable<Subscription>
    {

        public string ServiceUrl { get; set; }
        public string PlatformId { get; set; }
        public string Target { get; set; }
        public TargetType TargetType { get; internal set; }
        public ChannelAccount Subscriber { get; set; }
        public ChannelAccount Bot { get; set; }
        public IEnumerable<string> Tags { get; set; }

        public bool Equals(Subscription other)
        {
            return this.PlatformId == other.PlatformId
                && this.Target == other.Target
                && this.Bot.Id == other.Bot.Id
                && this.Bot.Name == other.Bot.Name
                && this.Bot.Role == other.Bot.Role
                && this.Subscriber.Id == other.Subscriber.Id
                && this.Subscriber.Name == other.Subscriber.Name
                && this.Subscriber.Role == other.Subscriber.Role
                && this.ServiceUrl == other.ServiceUrl;
        }

        public async Task Send(string appId, string appPassword, Question question)
        {
            var message = $"# [{question.Title}]({question.Link})\n\n\r\n";
            if (question.AnswerCount > 0)
            {
                message += $"Answered | ";
            }
            message += $"Asked by [{question.Owner.DisplayName} ({question.Owner.Reputation})]({question.Owner.Link})";
            await this.Send(appId, appPassword, message);
        }

        public async Task Send(string appId, string appPassword, string message)
        {
            var connector = new ConnectorClient(new Uri(this.ServiceUrl), new MicrosoftAppCredentials(appId, appPassword));

            var messageActivity = Activity.CreateMessageActivity();
            messageActivity.From = this.Bot;
            messageActivity.Recipient = this.Subscriber;
            messageActivity.Text = message;
            messageActivity.TextFormat = TextFormatTypes.Markdown;
            messageActivity.Locale = "en-GB";
            messageActivity.ChannelId = this.PlatformId;

            if (this.TargetType == TargetType.Conversation)
            {
                messageActivity.Conversation = new ConversationAccount(id: this.Target);
                await connector.Conversations.SendToConversationAsync((Activity) messageActivity);
            }
            else if (this.TargetType == TargetType.TeamsChannel)
            {
                var conversationParameters = new ConversationParameters
                {
                    IsGroup = true,
                    ChannelData = new TeamsChannelData
                    {
                        Channel = new ChannelInfo(this.Target),
                    },
                    Activity = (Activity)messageActivity
                };
                await connector.Conversations.CreateConversationAsync(conversationParameters);
            }

        }

        public void UpdateTags(IEnumerable<string> tags)
        {
            if (tags.Any())
                this.Tags = this.Tags.Union(tags);
            else
                this.Tags = new List<string>();
        }

    }
}
