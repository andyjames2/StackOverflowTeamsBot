using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackOverflowBot.Bots;
using StackOverflowBot.Commands.CommandHandling;
using StackOverflowBot.Querying;
using StackOverflowBot.Links;
using StackOverflowBot.Repositories;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();
            services.AddTransient<IBot, Bot>();
            services.AddSingleton(new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate}));
            services.AddSingleton<IStackOverflowMonitor, StackOverflowMonitor>();
            services.AddSingleton<ICommandBroker, CommandBroker>();
            services.AddSingleton<ICommandFactory, CommandFactory>();
            services.AddSingleton<ICommandHistory, CommandHistory>();
            services.AddSingleton<IRepository<Subscription>, PersistedInMemoryRepository<Subscription>>();
            services.AddSingleton<IRepository<Link>, PersistedInMemoryRepository<Link>>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IStackOverflowMonitor stackOverflowMonitor)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseMvc(route => {
                route.MapRoute("default", "{controller}/{action}", new { controller = "default", action = "index" });
                route.MapRoute("so", "so/{action}/{registrationKey}", new { controller = "stackoverflow", action = "index" });
            });

            stackOverflowMonitor.Start();
        }

    }
}
