using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Risk.Shared;
using Maksad_Client.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Maksad_Client.Pages
{
    public class PlayByPlayModeModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly IMemoryCache memoryCache;
        public PlayByPlayModel playByPlayModel;

        /*public PlayByPlayModeModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, PlayByPlayModel playByPlayModel)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.playByPlayModel = playByPlayModel;
        }*/

        public PlayByPlayModeModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.memoryCache = memoryCache;
        }

        public GameStatus Status { get; set; }


        public GameStatus CurrentStatus { get; set; }

        //public List<GameStatus> GameStatusList = new List<GameStatus>();

        public int MaxRow { get; private set; }
        public int MaxCol { get; private set; }

        public const string ListIndex = "Counter";

        public async Task OnGetAsync()
        {

            //Check if it exists in the cache
            List<GameStatus> GameStatusList;

            if (memoryCache.TryGetValue("Status", out GameStatusList))
            {
                CurrentStatus = GameStatusList[(int)HttpContext.Session.GetInt32(ListIndex)];
                MaxRow = CurrentStatus.Board.Max(t => t.Location.Row);
                MaxCol = CurrentStatus.Board.Max(t => t.Location.Column);
            }
            else
            {
                GameStatusList = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<List<GameStatus>>($"{configuration["GameServer"]}/playByPlay");


                CurrentStatus = GameStatusList[0];

                MaxRow = CurrentStatus.Board.Max(t => t.Location.Row);
                MaxCol = CurrentStatus.Board.Max(t => t.Location.Column);


                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(360);
                memoryCache.Set("Status", GameStatusList, cacheEntryOptions);
                HttpContext.Session.SetInt32(ListIndex, 0);
            }
        }


        public IActionResult OnPostPlayByPlayAsync(string action)
        {
            /*Implement  
             forwardOne,
             backwardOne,
             forwardEnd,
             backwardStart*/


            var GameStatusList = memoryCache.Get<List<GameStatus>>("Status");
            int CurrentIndex = (int) HttpContext.Session.GetInt32(ListIndex);
        
            if(action == "forwardOne")
            {   
                CurrentIndex++;
                CurrentStatus = GameStatusList[CurrentIndex];
                HttpContext.Session.SetInt32(ListIndex, CurrentIndex);
            }
            else if (action == "forwardEnd")
            {
                CurrentIndex = GameStatusList.Count - 1;
                CurrentStatus = GameStatusList[CurrentIndex];
                
                HttpContext.Session.SetInt32(ListIndex, CurrentIndex);
            }
            else if (action == "backwardOne")
            {
                if (CurrentIndex != 0)
                {
                    CurrentIndex--;
                    CurrentStatus = GameStatusList[CurrentIndex];

                    HttpContext.Session.SetInt32(ListIndex, CurrentIndex);
                }
            }else if (action == "backwardStart")
            {
                CurrentIndex = 0;
                CurrentStatus = GameStatusList[CurrentIndex];
                HttpContext.Session.SetInt32(ListIndex, CurrentIndex);
            }

            return RedirectToPage();
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
