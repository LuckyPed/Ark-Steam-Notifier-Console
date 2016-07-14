using Microsoft.Owin.Hosting;
using System;
using System.Net.Http;
using SteamKit2;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ArkSteamNotifier
{


    public class Program
    {

        public class Settings
        {
            public List<ulong> UnSub { get; set; }
            //Just a Default Value, you can change it later from here or by editing the settings.txt in the app folder.
            public string GameServerAddress { get; set; } = "66.147.230.53";
        }

        // Create our steamclient instance
        public static SteamClient steamClient = new SteamClient();
        static CallbackManager manager;
        static SteamUser steamUser;
        public static SteamFriends steamFriends;
        static bool isRunning;
        static string user, pass;
        static string authCode, twoFactorAuth;
        public static BackgroundWorker clientRunning = new BackgroundWorker();
        public static Settings settings = new Settings();

        static void Main()
        {
            
            if (File.Exists("settings.txt"))
            {
                string settingsString = File.ReadAllText("settings.txt");
                settings = JsonConvert.DeserializeObject<Settings>(settingsString);
            }



                clientRunning.WorkerSupportsCancellation = true;
            clientRunning.DoWork += new DoWorkEventHandler(clientRunning_DoWork);

            string baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {

                Console.WriteLine("Owin Host is Active, Listenting to POST  Requests at " + baseAddress);
                Console.WriteLine("Write 'Stop' to stop the Program.");
                Console.WriteLine("Do you Want to Login to Steam for SteamChat Notifiers ? Write 'LoginSteam' to login.");

                //Work Till recieve Stop command
                while (true)
                {
                    string respond = Console.ReadLine().ToLower();
                    if (respond == "stop")
                    {
                        break;
                    }
                    else if (respond == "loginsteam")
                    {

                        Console.Write("Enter UserName : ");
                        user = Console.ReadLine();
                        Console.Write("Enter Passwrod : ");
                        pass = Console.ReadLine();

                        if (clientRunning.IsBusy != true)
                        {
                            clientRunning.RunWorkerAsync();
                        }

                    }
                    else if (respond == "logoutsteam")
                    {
                        clientRunning.CancelAsync();
                    }
                }
            }
        }



        private static void clientRunning_DoWork(object sender, DoWorkEventArgs e)
        {

            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            // get the steam friends handler, which is used for interacting with friends on the network after logging on
            steamFriends = steamClient.GetHandler<SteamFriends>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);


            // we use the following callbacks for friends related activities
            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
            manager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);
            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMessage);



            isRunning = true;


            Console.WriteLine("Connecting to Steam...");

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while (isRunning)
            {
                if (clientRunning.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));

            }
        }


        public static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine(string.Format("Unable to connect to Steam: {0}", callback.Result));
                isRunning = false;
                return;
            }

            Console.WriteLine(string.Format("Connected to Steam! Logging in '{0}'...", user));

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = authCode,
                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = twoFactorAuth,
                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });
        }
        public static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            if (!clientRunning.CancellationPending)
            {
                Console.WriteLine("Disconnected from Steam, reconnecting in 5...");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                steamClient.Connect();
            }

        }
        public static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isEmail = callback.Result == EResult.AccountLogonDenied;
            bool isMobile = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;


            if (isEmail || isMobile)
            {
                Console.WriteLine("This account is SteamGuard protected!");

                if (isMobile)
                {
                    Console.WriteLine("Your account need mobile authenticator code.");
                    Console.Write("Press Enter first, Then enter your mobile authenticator code : ");
                    twoFactorAuth = Console.ReadLine();

                }
                else
                {
                    Console.Write(string.Format("Please Enter first,then enter the auth code sent to the email at {0} : ", callback.EmailDomain));
                    authCode = Console.ReadLine();
                }

                return;
            }

            if (callback.Result != EResult.OK)
            {
                Console.WriteLine(string.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult));
                isRunning = false;
                return;
            }

            Console.WriteLine("Successfully logged on!");

            // at this point, we'd be able to perform actions on Steam
        }
        public static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine(string.Format("Logged off of Steam: {0}", callback.Result));
        }
        public static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);

            // write out our sentry file
            int fileSize;
            using (var fs = File.OpenWrite("sentry.bin"))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;
            }

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Done!");

        }
        public static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            // before being able to interact with friends, you must wait for the account info callback
            // this callback is posted shortly after a successful logon

            // at this point, we can go online on friends, so lets do that
            steamFriends.SetPersonaState(EPersonaState.Online);
        }
        public static void OnFriendMessage(SteamFriends.FriendMsgCallback callback)
        {
            string callBackMessage = callback.Message.ToLower();

            if(callBackMessage == "sub")
            {
                //Remove From UnSubsriber's List
                settings.UnSub.Remove(callback.Sender.AccountID);
                string settingString = JsonConvert.SerializeObject(settings);
                File.WriteAllText("settings.txt", settingString);

                steamFriends.SendChatMessage(callback.Sender, callback.EntryType, "You have Subscribed to Ark Tribe Event Notification Bot.");
                steamFriends.SendChatMessage(callback.Sender, callback.EntryType, "If you wish to UnSubscribe, Reply with 'UnSub'.");
            }
            else if(callBackMessage == "unsub")
            {
                //Add to UnSubscriber's List
                settings.UnSub.Add(callback.Sender.AccountID);
                string settingString = JsonConvert.SerializeObject(settings);
                File.WriteAllText("settings.txt", settingString);

                steamFriends.SendChatMessage(callback.Sender, callback.EntryType, "You have UnSubscribed to Ark Tribe Event Notification Bot.");
                steamFriends.SendChatMessage(callback.Sender, callback.EntryType, "If you wish to Subscribe, Reply with 'Sub'.");
            }
            else
            {
                steamFriends.SendChatMessage(callback.Sender, callback.EntryType, "This is Ark Tribe Event Notification Bot for server " + settings.GameServerAddress);
                steamFriends.SendChatMessage(callback.Sender, callback.EntryType, "If your wish to UnSubscribe reply 'UnSub'.");
            }

        }
        public static void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            int friendCount = steamFriends.GetFriendCount();
            Console.WriteLine(string.Format("We have {0} friends", friendCount));
         
            //Accept all pending friend requests
            foreach (var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    steamFriends.AddFriend(friend.SteamID);
                }
            }
        }
        public static void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            //Accept new friend requests
            steamFriends.AddFriend(callback.SteamID);
            Console.WriteLine(string.Format("{0} is now a friend", callback.PersonaName));
        }

    }
}
