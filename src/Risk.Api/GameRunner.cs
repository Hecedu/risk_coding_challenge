using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Risk.Game;
using Risk.Shared;

namespace Risk.Api
{
    public class GameRunner
    {
        private readonly Game.Game game;
        private readonly IList<ApiPlayer> removedPlayers;
        private readonly ILogger<GameRunner> logger;
        public const int MaxFailedTries = 5;

        

        public GameRunner(Game.Game game, ILogger<GameRunner> logger)
        {
            this.game = game;
            this.removedPlayers = new List<ApiPlayer>();
            this.logger = logger;
        }

        public async Task StartGameAsync()
        {
            game.TakeGameSnapshot("Game start!");

            await deployArmiesAsync();
            await doBattle();
            await reportWinner();
        }


        //TODo: As soon as someone deploys, record the action as GameState

        private async Task deployArmiesAsync()
        {
            while (game.Board.Territories.Sum(t => t.Armies) < game.StartingArmies * game.Players.Count())
            {
                for (int playerIndex = 0; playerIndex < game.Players.Count(); ++playerIndex)
                {
                    var currentPlayer = game.Players.Skip(playerIndex).First() as ApiPlayer;
                    var deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.YourTurn);
                    var failedTries = 0;
                    //check that this location exists and is available to be used (e.g. not occupied by another army)
                    while (game.TryPlaceArmy(currentPlayer.Token, deployArmyResponse.DesiredLocation) is false)
                    {
                        failedTries++;
                        if (failedTries == MaxFailedTries)
                        {
                            BootPlayerFromGame(currentPlayer);
                            playerIndex--;
                            break;
                        }
                        else
                        {
                            deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.PreviousAttemptFailed);
                        }
                    }
                    
                    var message = $"{currentPlayer.Name}  deployed to {deployArmyResponse.DesiredLocation}";

                    logger.LogDebug(message);

                    
                    //Record the GameStatus****************************
                    game.TakeGameSnapshot(message);
                }
            }
        }


        private async Task<DeployArmyResponse> askForDeployLocationAsync(ApiPlayer currentPlayer, DeploymentStatus deploymentStatus)
        {
            var deployArmyRequest = new DeployArmyRequest {
                Board = game.Board.SerializableTerritories,
                Status = deploymentStatus,
                ArmiesRemaining = game.GetPlayerRemainingArmies(currentPlayer.Token)
            };
            var json = System.Text.Json.JsonSerializer.Serialize(deployArmyRequest);
            var deployArmyResponse = (await currentPlayer.HttpClient.PostAsJsonAsync("/deployArmy", deployArmyRequest));
            deployArmyResponse.EnsureSuccessStatusCode();
            var r = await deployArmyResponse.Content.ReadFromJsonAsync<DeployArmyResponse>();
            return r;
        }


        //TODo: As soon as someone attacks, record the action as GameState
        private async Task doBattle()
        {
            game.StartTime = DateTime.Now;
            string message;

            while (game.Players.Count() > 1 && game.GameState == GameState.Attacking && game.Players.Any(p=>game.PlayerCanAttack(p)))
            {

                for (int i = 0; i < game.Players.Count() && game.Players.Count() > 1; i++)
                {
                    var currentPlayer = game.Players.Skip(i).First() as ApiPlayer;
                    if (game.PlayerCanAttack(currentPlayer))
                    {
                        var failedTries = 0;

                        TryAttackResult attackResult = new TryAttackResult {  AttackInvalid = false} ;
                        Territory attackingTerritory = null;
                        Territory defendingTerritory = null;
                        do
                        {
                            logger.LogInformation($"Asking {currentPlayer.Name} where they want to attack...");

                            var beginAttackResponse = await askForAttackLocationAsync(currentPlayer, BeginAttackStatus.PreviousAttackRequestFailed);
                            try
                            {
                                attackingTerritory = game.Board.GetTerritory(beginAttackResponse.From);
                                defendingTerritory = game.Board.GetTerritory(beginAttackResponse.To);

                                message = $"{currentPlayer.Name} attacked from {attackingTerritory} to {defendingTerritory}";
                                logger.LogInformation(message);

                                attackResult = game.TryAttack(currentPlayer.Token, attackingTerritory, defendingTerritory);

                                //Record the Game Status *****************************
                                game.TakeGameSnapshot(message);
                            }
                            catch (Exception ex)
                            {
                                attackResult = new TryAttackResult { AttackInvalid = true, Message=ex.Message };
                            }
                            if (attackResult.AttackInvalid)
                            {
                                message = ($"Invalid attack request! {currentPlayer.Name} from {attackingTerritory} to {defendingTerritory} ");
                                game.TakeGameSnapshot(message);

                                failedTries++;
                                if (failedTries == MaxFailedTries)
                                {
                                    BootPlayerFromGame(currentPlayer);
                                    i--;
                                    break;
                                }
                            }
                        } while (attackResult.AttackInvalid);

                        while (attackResult.CanContinue)
                        {
                            var continueResponse = await askContinueAttackingAsync(currentPlayer, attackingTerritory, defendingTerritory);
                            if (continueResponse.ContinueAttacking)
                            {
                                message = "Keep attacking!";
                                logger.LogInformation(message);
                                attackResult = game.TryAttack(currentPlayer.Token, attackingTerritory, defendingTerritory);
                                game.TakeGameSnapshot(message);
                            }
                            else
                            {
                                logger.LogInformation("run away!");
                                break;
                            }
                        }
                    }
                    else
                    {
                        message = ($"{currentPlayer.Name} cannot attack.");
                        game.TakeGameSnapshot(message);
                    }
                }


            }

            message = "Game Over";
            logger.LogInformation(message);
            game.SetGameOver();
            game.TakeGameSnapshot(message);

        }

       
        private void RemovePlayerFromGame(string token)
        {
            var player = game.RemovePlayerByToken(token) as ApiPlayer;
            removedPlayers.Add(player);

            var  message = ($"{player.Name} was removed from game");
            game.TakeGameSnapshot(message);
        }

        private async Task<BeginAttackResponse> askForAttackLocationAsync(ApiPlayer player, BeginAttackStatus beginAttackStatus)
        {
            var beginAttackRequest = new BeginAttackRequest {
                Board = game.Board.SerializableTerritories,
                Status = beginAttackStatus
            };
            return await (await player.HttpClient.PostAsJsonAsync("/beginAttack", beginAttackRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<BeginAttackResponse>();
        }

        private async Task reportWinner()
        {
            game.EndTime = DateTime.Now;
            TimeSpan gameDuration = game.EndTime - game.StartTime;

            var scores = new List<(int score, ApiPlayer player)>();

            foreach (ApiPlayer currentPlayer in game.Players)
            {
                var playerScore = 2 * game.GetNumTerritories(currentPlayer) + game.GetNumPlacedArmies(currentPlayer);

                scores.Add((playerScore, currentPlayer));
            }

            var orderedScores = scores.OrderByDescending(s => s.score);

            var gameOverRequest = new GameOverRequest {
                FinalBoard = game.Board.SerializableTerritories,
                GameDuration = gameDuration.ToString(),
                WinnerName = orderedScores.First().player.Name,
                FinalScores = orderedScores.Select(s => $"{s.player.Name} ({s.score})")
            };

            foreach (ApiPlayer currentPlayer in game.Players)
            {
                var response = await (currentPlayer.HttpClient.PostAsJsonAsync("/gameOver", gameOverRequest));
            }
        }

        public bool IsAllArmiesPlaced()
        {
            int playersWithNoRemaining = game.Players.Count(p => game.GetPlayerRemainingArmies(p.Token) == 0);

            return (playersWithNoRemaining == game.Players.Count());
        }


        
        public void RemovePlayerFromBoard(String token)
        {
            foreach (Territory territory in game.Board.Territories)
            {
                if (territory.Owner == game.GetPlayer(token))
                {
                    territory.Owner = null;
                    territory.Armies = 0;
                }
            }
        }

        private async Task<ContinueAttackResponse> askContinueAttackingAsync(ApiPlayer currentPlayer, Territory attackingTerritory, Territory defendingTerritory)
        {
            var continueAttackingRequest = new ContinueAttackRequest {
                Board = game.Board.SerializableTerritories,
                AttackingTerritorry = attackingTerritory,
                DefendingTerritorry = defendingTerritory
            };
            var continueAttackingResponse = await (await currentPlayer.HttpClient.PostAsJsonAsync("/continueAttacking", continueAttackingRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<ContinueAttackResponse>();
            return continueAttackingResponse;
        }

        public void BootPlayerFromGame(ApiPlayer player)
        {
            RemovePlayerFromBoard(player.Token);
            RemovePlayerFromGame(player.Token);

            var message = $"{player.Name} was booted from the game! BYE...";
            game.TakeGameSnapshot(message);
        }
    }
}
