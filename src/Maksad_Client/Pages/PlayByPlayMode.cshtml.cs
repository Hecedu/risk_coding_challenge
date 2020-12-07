using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Maksad_Client.Pages
{
    public class PlayByPlayModeModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public PlayByPlayModeModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public GameStatus Status { get; set; }


        public GameStatus CurrentStatus { get; set; }

        public List<GameStatus> GameStatusList = new List<GameStatus>();

        public int MaxRow { get; private set; }
        public int MaxCol { get; private set; }

        public int ListIndex { get; set; }

        public async Task OnGetAsync()
        {
            Status = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<GameStatus>($"{configuration["GameServer"]}/status");


            GameStatusList = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<List<GameStatus>>($"{configuration["GameServer"]}/playByPlay");

            MaxRow = Status.Board.Max(t => t.Location.Row);
            MaxCol = Status.Board.Max(t => t.Location.Column);
        }


        public async Task OnPostPlayByPlayAsync(string action)
        {
           /*Implement  
            forwardOne,
            backwardOne,
            forwardEnd,
            backwardStart*/

            foreach (var gameStatus in GameStatusList.Select((x, i) => new { Value = x, Index = i }))
            {
                
            }

        }

        public async Task<IActionResult> StartGameAsync()
        {
            var client = httpClientFactory.CreateClient();
            Task.Run(() =>
                client.PostAsJsonAsync($"{configuration["GameServer"]}/startgame", new StartGameRequest { SecretCode = configuration["secretCode"] })
            );
            return new RedirectToPageResult("GameStatus");
        }
    }
}
