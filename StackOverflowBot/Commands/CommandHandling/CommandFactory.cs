using System;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackOverflowBot.Links;
using StackOverflowBot.Subscriptions;
using StackOverflowBot.Repositories;
using System.Net.Http;

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
            var linkRepo = this._serviceProvider.GetService<IRepository<Link>>();
            var configuration = this._serviceProvider.GetService<IConfiguration>();
            switch (command.ToLower())
            {
                case "latest":
                    var httpClient = this._serviceProvider.GetService<HttpClient>();
                    return new LatestCommand(httpClient, configuration.GetValue<string>("StackOverflowKey"), linkRepo, turnContext, cancellationToken);
                case "subscribe":
                    return new SubscribeCommand(subscriptionRepo, turnContext, cancellationToken);
                case "unsubscribe":
                    return new UnsubscribeCommand(subscriptionRepo, turnContext, cancellationToken);
                case "link":
                    var rootUrl = configuration.GetValue<string>("RootUrlForLinks");
                    return new LinkCommand(rootUrl, linkRepo, turnContext, cancellationToken);
                case "unlink":
                    return new UnlinkCommand(linkRepo, turnContext, cancellationToken);
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
