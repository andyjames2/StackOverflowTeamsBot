using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackOverflowBot.Bots;
using StackOverflowBot.Registrations;
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
            services.AddSingleton<IRepository<Subscription>, InMemoryRepository<Subscription>>();
            services.AddSingleton<IRepository<Registration>, InMemoryRepository<Registration>>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseMvc(route => {
                route.MapRoute("default", "{controller}/{action}", new { controller = "default", action = "index" });
                route.MapRoute("so", "so/{action}/{registrationKey}", new { controller = "stackoverflow", action = "index" });
            });
        }

    }
}
