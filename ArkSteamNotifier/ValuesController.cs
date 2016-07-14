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

        public void Post1(string key, ulong steamid, string notetitle, string message)
        {
            if (!Program.settings.UnSub.Contains(steamid))
            {
                SteamID SID = new SteamID(steamid);
                Program.steamFriends.SendChatMessage(SID, EChatEntryType.ChatMsg, System.DateTime.UtcNow + " UTC ( GMT ) : " + notetitle);
                Thread.Sleep(1000);
                Program.steamFriends.SendChatMessage(SID, EChatEntryType.ChatMsg, message);
            }
        }

        public void Post([FromBody]string key,[FromBody]ulong steamid,[FromBody]string notetitle,[FromBody]string message)
        {
            if (!Program.settings.UnSub.Contains(steamid))
            {
                SteamID SID = new SteamID(steamid);
                Program.steamFriends.SendChatMessage(SID, EChatEntryType.ChatMsg, System.DateTime.UtcNow + " UTC ( GMT ) : " + notetitle);
                Thread.Sleep(1000);
                Program.steamFriends.SendChatMessage(SID, EChatEntryType.ChatMsg, message);
            }
        }

        public class MyFormData
        {
            public string Data { get; set; }
        }

        public void Post([FromBody]MyFormData formData)
        {
            //your JSON string will be in formData.Data
        }


        public void Post([FromBody]MyData formData)
        {

        }

        public class MyData
        {
            public string key { get; set; }
            public string steamid { get; set; }
            public string message { get; set; }
            public string notetitle { get; set; }
        }

        public void Post(string value)
        {

        }

        public void Post1([FromBody]string value)
        {

        }
        
    }
}
