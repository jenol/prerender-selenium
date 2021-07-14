using Akka.Actor;
using Akka.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PrerenderDotNet.Actors;
using PrerenderDotNet.Clients;
using System.Linq;

namespace PrerenderDotNet
{
    public class Startup
    {
        private ActorSystem system;

        public Startup(IConfiguration configuration)
        {            
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            system = ActorSystem.Create("MySystem");

            var client = new ProcessClient("p");
            client.DeleteOrphanedProcesses();

            services.AddSingleton<IProcessClient>(s => client);

            var activityObserverRef = system.ActorOf(BrowserObserverActor.Props(client));

            var workerNames = Enumerable.Range(1, 5).Select(n => $"worker-{n}");
            var workers = workerNames.Select(name => system.ActorOf(PrerenderActor.Props(activityObserverRef), name));
            var group = new RoundRobinGroup(workers.Select(w => w.Path.ToStringWithAddress()));
            var groupRef = system.ActorOf(Props.Empty.WithRouter(group));

            services.AddSingleton<IRenderingClient>(s => new RenderingClient(groupRef));
            services.AddSingleton<IBrowserClient>(s => new BrowserClient(activityObserverRef)); 
            services.AddControllers();
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "PrerenderDotNet", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, IWebHostEnvironment env)
        {
            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();                
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PrerenderDotNet v1"));
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private void OnShutdown()
        {
            system?.Dispose();
        }
    }
}
