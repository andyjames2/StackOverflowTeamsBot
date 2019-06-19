using System;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackOverflowBot.Registrations;
using StackOverflowBot.Subscriptions;
using StackOverflowBot.Repositories;

namespace StackOverflowBot.Commands.CommandHandling
{
    class CommandFactory : ICommandFactory
    {

        private readonly IServiceProvider _serviceProvider;

        public CommandFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public ICommand Create(string command, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var subscriptionRepo = this._serviceProvider.GetService<IRepository<Subscription>>();
            switch (command.ToLower())
            {
                case "subscribe":
                    return new SubscribeCommand(subscriptionRepo, turnContext, cancellationToken);
                case "unsubscribe":
                    return new UnsubscribeCommand(subscriptionRepo, turnContext, cancellationToken);
                case "register":
                    var repository = this._serviceProvider.GetService<IRepository<Registration>>();
                    var config = this._serviceProvider.GetService<IConfiguration>();
                    var rootUrl = config.GetValue<string>("RootUrlForLinks");
                    return new RegisterCommand(rootUrl, repository, turnContext, cancellationToken);
                case "undo":
                    var commandHistory = this._serviceProvider.GetService<ICommandHistory>();
                    return new UndoCommand(commandHistory, turnContext, cancellationToken);
                case "help":
                    return new HelpCommand(turnContext, cancellationToken);
                default:
                    return new NoCommand(turnContext, cancellationToken);
            }
        }

    }
}
