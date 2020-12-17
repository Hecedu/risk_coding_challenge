﻿using System;
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

namespace Visualizer.Pages
{
    public class Visualizer : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public Visualizer(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public GameStatus Status { get; set; }
        public int MaxRow { get; private set; }
        public int MaxCol { get; private set; }


        public async Task OnGetAsync()
        {
            Status = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<GameStatus>($"{configuration["GameServer"]}/status");
            MaxRow = Status.Board.Max(t => t.Location.Row);
            MaxCol = Status.Board.Max(t => t.Location.Column);
        }


        public async Task<IActionResult> OnPostRestartGame()
        {
            var client = httpClientFactory.CreateClient();
            await Task.Run(() =>
                client.PostAsJsonAsync($"{configuration["GameServer"]}/restartgame", new StartGameRequest { SecretCode = configuration["secretCode"] })
            );
            StartGame();
            
            return RedirectToPage("Visualizer");
        }

        public async Task StartGame()
        {
            var client = httpClientFactory.CreateClient();
            await Task.Run(() =>
                client.PostAsJsonAsync($"{configuration["GameServer"]}/startgame", new StartGameRequest { SecretCode = configuration["secretCode"] })
            );
        }

    }
}