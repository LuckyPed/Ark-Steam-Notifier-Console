# Ark-Steam-Notifier-Console
A console application for making a steam bot providing immediate Ark : Survival Evolved Tribe Log Notification using steam chat system.

## How does this bot works :

Well, This Bot is made of 2 Major Section,

**First**, It's the OwinSelfHost Package which let us Recieve the HTML POST Request sent to an Specific address (Localhost:9000 for example) by Ark : Survival Evolved's Game Server which we are running.

**Then**, It's the SteamKit2 Package which let us login to steam client, Auto-Accept steam friend requests and send the Post Requests we recieved to the steam user which it belong !


## What Libraries are required for using this ?

It's **OwinSelfHost** and **SteamKit2** Packages with their dependencies.

## What I need to do before i can use it on my Ark Server

You only need to Download And Compile, I will later add a compiled zip file too for easy download,
Then run the ArkSteamNotifier.exe and login to the steam account you want to be your Bot.

Don't forget you need to run your Ark Server with **-webalarms** commandline and have **AlarmPostCredentials.txt** in your **ShooterGame\Saved** directory with the content bellow for it to send the POST requests correctly.

2 Line, First is a Key which you can choose randomly,Secound is the URL which the POST Request will be sent.
>1232fje6d5f8r123456789

>http://localhost:9000/api/values/?

## Can I use this on Official Servers ?

Sadly, No you can't use it on Official Servers as their notifications only go to **http://SurviveTheArk.com**

## Is there a Server to test this and see if i like it ?
Yes, Currently I run a server which have this Feature working on it,
You can join and Test it in here : [**steam://connect/66.147.230.53:27015**](steam://connect/66.147.230.53:27015)

## What Notifications I actully recieve right now ?
As of now, only news about Tripwire going off or baby dinos being born will be notified to you by the bot.

But Ark developers already said that they will soon change it to whole tribe log, and as soon as they do it, my bot will also notify about all tribe log.

## Does my Server's Player need to add the bot to recieve the notifications ?
Yes, they all need to add the bot first to recieve notification,
By a really simple change anyone can make the bot Auto-Add the Players the first time they recieve a Tribe Notification. But right now bot only auto acccept.

## Is there going to be a Remote Version which we don't need to run on the dedicated server itself ?
I don't know, I have never made a Remote application, I need to search and see what programming skill I need for that,
But making a Graphical Version of this is really easy and I might make a Windows Form or WPF Version Soon.


###Enjoy The Ark :)
