using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    /// <summary>
    /// The server sends this to the client, asking them which action they want to perform (attack or pacifism)
    /// </summary>
    public class ActionRequest
    {
        public ActionStatus Status { get; set; }
    }
    public enum ActionStatus
    {
        YourTurn,
        PreviousActionRequestFailed
    }
}
