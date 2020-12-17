using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Risk.Shared;
using System.Net.Http.Json;

namespace Emmanuel_Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpClient();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            //var server = Configuration["ServerName"];
            //var httpClient = httpClientFactory.CreateClient();
            //var clientBaseAddress = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.ToArray()[1];

            JoinServer(httpClientFactory.CreateClient(), Configuration["GameServer"], Configuration["ClientCallbackAddress"], Configuration["ClientName"]);
        }

        private async Task JoinServer(HttpClient httpClient, string serverName, string clientBaseAddress, string clientName)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            var joinRequest = new JoinRequest { Name = clientName, CallbackBaseAddress = clientBaseAddress };
            var joinResponse = await httpClient.PostAsJsonAsync($"{serverName}/join", joinRequest);
        }
    }
}
