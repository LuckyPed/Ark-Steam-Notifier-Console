using System.Collections.Generic;
using System.Web.Http;
using SteamKit2;
using System.Threading;

namespace ArkSteamNotifier
{
    public class ValuesController : ApiController
    {
        // GET api/values 
        public string Get()
        {
            return "This Page is for Reciving POST Data from ARK Game Server";
        }

        public void Post(string key, ulong steamid, string notetitle, string message)
        {
            if (!Program.settings.UnSub.Contains(steamid))
            {
                SteamID SID = new SteamID(steamid);
                Program.steamFriends.SendChatMessage(SID, EChatEntryType.ChatMsg, System.DateTime.UtcNow + " UTC ( GMT ) : " + notetitle);
                Thread.Sleep(1000);
                Program.steamFriends.SendChatMessage(SID, EChatEntryType.ChatMsg, message);
            }
        }

    }
}
