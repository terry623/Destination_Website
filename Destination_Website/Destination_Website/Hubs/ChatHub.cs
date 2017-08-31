using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Destination_Website.Hubs
{
    public class ChatHub : Hub
    {
        public void Send(string target_Id, string message)
        {

        }
    }
}