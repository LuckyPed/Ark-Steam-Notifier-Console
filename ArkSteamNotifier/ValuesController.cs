using System.Collections.Generic;
using System.Web.Http;
using SteamKit2;
using System.Threading;
using System;
using System.Linq;

namespace ArkSteamNotifier
{
    public class ValuesController : ApiController
    {
        // GET api/values 
        public string Get()
        {
            return "This Page is for Reciving POST Data from ARK Game Server";
        }

        public class SteamData
        {
            public string key { get; set; }
            public ulong steamid { get; set; }
            public string notetitle { get; set; }
            public string message { get; set; }
        }

        public void Post([FromBody] SteamData data)
        {
            if (!Program.settings.UnSub.Contains(data.steamid))
            {
                SteamID SID = new SteamID(data.steamid);               
                Program.steamFriends.SendChatMessage(SID, EChatEntryType.ChatMsg, System.DateTime.UtcNow + " UTC ( GMT ) : " + data.notetitle);

                //No Need to Send the Message body atm since it's only '...' and nothing more yet :)
                //Thread.Sleep(1000);
                //Program.steamFriends.SendChatMessage(SID, EChatEntryType.ChatMsg, data.message);
            }
        }


    }
}
