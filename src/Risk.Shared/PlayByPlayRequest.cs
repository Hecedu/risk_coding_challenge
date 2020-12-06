using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    class PlayByPlayRequest
    {
        public PlayByPlayOptions RequestOption { get; set; }
    }

    public enum PlayByPlayOptions
    {
        ForwardOneStep,
        BackwardOneStep,
        ForwardTillEnd,
        BackwardTillStart
    }

}
