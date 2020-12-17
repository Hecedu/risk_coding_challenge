using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    /// <summary>
    /// The client sends this to the server telling the server which action he wants to perform
    /// </summary>
    public class ActionResponse
    {
        public UserAction userAction { get; set; }
    }
    public enum UserAction
    {
        Attack,
        Pacifism
    }
}
