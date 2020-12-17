using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Risk.Shared;

namespace Risk.Api.Controllers
{
    [ApiController]
    public class GameController : Controller
    {
        private Game.Game game;

        private GameRunner gameRunner;
        private IMemoryCache memoryCache;
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration config;
        private readonly ILogger<GameRunner> logger;
        private readonly List<ApiPlayer> removedPlayers = new List<ApiPlayer>();

        public GameController(Game.Game game, IMemoryCache memoryCache, IHttpClientFactory client, IConfiguration config, ILogger<GameRunner> logger)
        {
            this.game = game;
            this.clientFactory = client;
            this.config = config;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        private async Task<bool> ClientIsRepsonsive(string baseAddress)
        {
            //client.CreateClient().BaseAddress = new Uri(baseAddress);
            var response = await clientFactory.CreateClient().GetStringAsync($"{baseAddress}/areYouThere");
            return response.ToLower() == "yes";
        }

        [HttpGet("status")]
        public IActionResult GameStatus()
        {
            GameStatus gameStatus;

            if (!memoryCache.TryGetValue("Status", out gameStatus))
            {
                gameStatus = game.GetGameStatus();
                if(gameStatus.GameState == GameState.Restarting)
                {
                    game.StartJoining();
                }
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
                memoryCache.Set("Status", gameStatus, cacheEntryOptions);
            }

            return Ok(gameStatus);
        }


        [HttpGet("playByPlay")]
        public IActionResult PlayByPlayOption()
        {
            return Ok(game.GameStatusList);
        }

        public static Game.Game InitializeGame (int height, int width, int numOfArmies)
        {
            GameStartOptions startOptions = new GameStartOptions {
                Height = height,
                Width = width,
                StartingArmiesPerPlayer = numOfArmies
            };
            Game.Game newGame = new Game.Game(startOptions);

            newGame.StartJoining();
            return newGame;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Join(JoinRequest joinRequest)
        {
            if (game.GameState == GameState.Joining && await ClientIsRepsonsive(joinRequest.CallbackBaseAddress))
            {
                var newPlayer = new ApiPlayer(
                    name: joinRequest.Name,
                    token: Guid.NewGuid().ToString(),
                    httpClient: clientFactory.CreateClient()
                );
                newPlayer.HttpClient.BaseAddress = new Uri(joinRequest.CallbackBaseAddress);

                game.AddPlayer(newPlayer);

                return Ok(new JoinResponse {
                    Token = newPlayer.Token
                });
            }
            else
            {
                return BadRequest("Unable to join game");
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> StartGame(StartGameRequest startGameRequest)
        {
            if(game.GameState == GameState.Restarting)
            {
                game.StartJoining();
            }

            if(game.GameState != GameState.Joining)

            if (game.GameState == GameState.Restarting)
            {
                game.StartJoining();
            }

            if (game.GameState != GameState.Joining)
            {
                return BadRequest("Game not in Joining state");
            }
            if(config["secretCode"] != startGameRequest.SecretCode)
            {
                return BadRequest("Secret code doesn't match, unable to start game.");
            }
            game.StartGame();
            var gameRunner = new GameRunner(game, logger);
            await gameRunner.StartGameAsync();

            GameOverRequest gameOverRequest = await gameRunner.reportWinner();

            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(360);
            memoryCache.Set("ReportWinner", gameOverRequest, cacheEntryOptions);
            //Create a session and store the gameRunner.winnerReport
            return Ok();
        }


        [HttpPost("[action]")]
        public IActionResult RestartGame(StartGameRequest startGameRequest)
        {
            if (game.GameState != GameState.GameOver)
            {
                return BadRequest("Game not finished");
            }
            if (config["secretCode"] != startGameRequest.SecretCode)
            {
                return BadRequest("Secret code doesn't match, unable to start game.");
            }

            game.Restarting();

            return Ok();
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GameOverStats()
        {

            if (game.GameState == GameState.GameOver)
            {
                GameOverRequest gameOverRequest;

                gameOverRequest = memoryCache.Get<GameOverRequest>("ReportWinner");
                return Ok(gameOverRequest);
            }

            return Ok();
        }
    }
}
