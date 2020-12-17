using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Risk.Shared;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using Risk.Shared;

namespace Visualizer.Pages


{
    public class gameOverModel : PageModel
    {
 
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;


        public GameOverRequest gameOverRequest { get; set; }

        public int MaxRow { get; private set; }
        public int MaxCol { get; private set; }

        public gameOverModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            gameOverRequest = await httpClientFactory
               .CreateClient()
               .GetFromJsonAsync<GameOverRequest>($"{configuration["GameServer"]}/GameOverStats");

            MaxRow = gameOverRequest.FinalBoard.Max(t => t.Location.Row);
            MaxCol = gameOverRequest.FinalBoard.Max(t => t.Location.Column);
        }
    }
}
