using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Risk.Shared;


namespace Visualizer.Pages
{
    public class Visualizer : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly IMemoryCache memoryCache;


        public Visualizer(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.memoryCache = memoryCache;
        }

        public GameStatus Status { get; set; }
        public int MaxRow { get; private set; }
        public int MaxCol { get; private set; }

        public GameOverRequest gameOverRequest { get; set; }



        public async Task OnGetAsync()
        {
            Status = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<GameStatus>($"{configuration["GameServer"]}/status");
            MaxRow = Status.Board.Max(t => t.Location.Row);
            MaxCol = Status.Board.Max(t => t.Location.Column);
            

            if(Status.GameState == GameState.GameOver)
            {
                gameOverRequest = await httpClientFactory
               .CreateClient()
               .GetFromJsonAsync<GameOverRequest>($"{configuration["GameServer"]}/GameOverStats");
            }
        }


        public async Task<IActionResult> OnPostRestartGame()
        {
            var client = httpClientFactory.CreateClient();
            await Task.Run(() =>
                client.PostAsJsonAsync($"{configuration["GameServer"]}/restartgame", new StartGameRequest { SecretCode = configuration["secretCode"] })
            );

            List<GameStatus> GameStatusList;

            if (memoryCache.TryGetValue("Status", out GameStatusList))
            {
                memoryCache.Remove("Status");
            }

            return RedirectToPage("JoinGame");
        }
      
    }
}