using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Risk.SampleClient.Controllers;
using System.Net.Http.Json;
using Risk.Shared;
using Microsoft.Extensions.Configuration;

namespace Risk.SampleClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ColorGenerator colorGenerator)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            ColorGenerator = colorGenerator;
        }

        public GameStatus Status { get; set; }
        public int MaxRow { get; private set; }
        public int MaxCol { get; private set; }
        public ColorGenerator ColorGenerator { get; }



        /*        public async Task OnGetAsync()
                {
                    Status = await httpClientFactory
                        .CreateClient()
                        .GetFromJsonAsync<GameStatus>($"{configuration["GameServer"]}/status");
                    MaxRow = Status.Board.Max(t => t.Location.Row);
                    MaxCol = Status.Board.Max(t => t.Location.Column);
                }*/

        
        public async Task<IActionResult> OnGetAsync()
        {

            string url = "http://localhost:5050/JoinGame";

            System.Uri uri = new System.Uri(url);

            return Redirect(uri.ToString());
        }

        public async Task<IActionResult> OnPostStartGameAsync()
        {
            var client = httpClientFactory.CreateClient();
            Task.Run(()=>
                client.PostAsJsonAsync($"{configuration["GameServer"]}/startgame", new StartGameRequest { SecretCode = configuration["secretCode"] })
            );            
            return new RedirectToPageResult("Index");
        }
    }
}
