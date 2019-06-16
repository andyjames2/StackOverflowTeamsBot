using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.DependencyInjection;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Commands
{

    public abstract class Command
    {

        public delegate Command CommandFactory(IServiceProvider serviceProvider, ITurnContext turnContext, CancellationToken cancellationToken);

        private static Dictionary<string, Stack<Command>> _history = new Dictionary<string, Stack<Command>>();
        private static Dictionary<string, CommandFactory> _actionMap = new Dictionary<string, CommandFactory>
        {
            {
                "subscribe", (serviceProvider, turnContext, cancellationToken) =>
                {
                    var subscriptionSchemeCollection = serviceProvider.GetService<IRepository<Subscription>>();
                    return new SubscribeCommand(subscriptionSchemeCollection, turnContext, cancellationToken);
                }
            },
            {
                "unsubscribe", (serviceProvider, turnContext, cancellationToken) =>
                {
                    var subscriptionSchemeCollection = serviceProvider.GetService<IRepository<Subscription>>();
                    return new UnsubscribeCommand(subscriptionSchemeCollection, turnContext, cancellationToken);
                }
            },
            {
                "undo", (serviceProvider, turnContext, cancellationToken) =>
                {
                    return new UndoCommand(_history, turnContext, cancellationToken);
                }
            }
        };

        public static CommandFactory Resolve(string command)
        {
            if (_actionMap.TryGetValue(command.ToLower(), out var factory))
                return factory;
            else
                return (serviceProvider, turnContext, cancellationToken) => new NoCommand(turnContext, cancellationToken);
        }

        public static async Task Execute(IServiceProvider serviceProvider, ITurnContext turnContext, CancellationToken cancellationToken, string commandStr, IEnumerable<string> args)
        {
            var command = Resolve(commandStr)(serviceProvider, turnContext, cancellationToken);
            var result = await command.Do(args);
            if (result)
            {
                var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
                var isTeamsChannel = channelData.Channel != null;
                string target;
                if (isTeamsChannel)
                    target = channelData.Channel.Id;
                else
                    target = turnContext.Activity.Conversation.Id;

                Stack<Command> historyStack;
                if (_history.ContainsKey(target))
                    historyStack = _history[target];
                else
                    _history[target] = historyStack = new Stack<Command>();

                historyStack.Push(command);
            }
        }

        public static async Task Execute()
        {

        }

        public abstract Task<bool> Do(IEnumerable<string> args);

        public abstract Task Undo();

    }

}
