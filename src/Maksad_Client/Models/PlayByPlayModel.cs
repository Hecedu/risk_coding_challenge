using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;

namespace Maksad_Client.Models
{
    public class PlayByPlayModel
    {
        public List<GameStatus> GameStatusList { get; set; }

        public int indexCounter { get; set; }
    }
}
