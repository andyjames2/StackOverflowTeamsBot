﻿using System;
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

        public async Task ReceiveQuestion(string appId, string appPassword, Question question)
        {
            var activity = Activity.CreateMessageActivity();
            activity.From = this.Bot;
            activity.Recipient = this.Subscriber;
            activity.Text = question.ToString();
            activity.TextFormat = TextFormatTypes.Markdown;
            activity.Locale = "en-GB";
            activity.ChannelId = this.PlatformId;

            await this.Send(appId, appPassword, (Activity) activity);
        }

        public async Task Send(string appId, string appPassword, Activity activity)
        {
            var connector = new ConnectorClient(new Uri(this.ServiceUrl), new MicrosoftAppCredentials(appId, appPassword));

            if (this.TargetType == TargetType.Conversation)
            {
                activity.Conversation = new ConversationAccount(id: this.Target);
                await connector.Conversations.SendToConversationAsync(activity);
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
                    Activity = activity
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
