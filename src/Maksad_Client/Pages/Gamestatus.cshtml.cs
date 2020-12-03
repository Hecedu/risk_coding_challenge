using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Maksad_Client.Pages
{
    public class GameStatusModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;
        

        public GameStatusModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
        }


        public async Task OnGetAsync()
        {
            var client = httpClientFactory.CreateClient();
            await refreshStatus(client);
        }

        private async Task refreshStatus(HttpClient client)
        {
            Status = await client.GetFromJsonAsync<GameStatus>($"{config["GameServer"]}/status");
        }

        public GameStatus Status { get; set; }

        public async Task OnPostStartGameAsync()
        {
            var client = httpClientFactory.CreateClient();
            await client.PostAsJsonAsync($"{config["GameServer"]}/startgame", new StartGameRequest { SecretCode = config["secretCode"]});
            
            await refreshStatus(client);
        }
    }
}
