# TwitchIRC
### An IRC class library for C#

It retrieves messages, sends messages, and handles the ping/ponging for you. I made this library to help me work Twitch integration into Unity games, but it works for any C# application.

### Prerequisites

.NET framework 2.0 or higher

## Usage

Here's a quick example of a console application that simply relays chat messages until the program is closed: 

```csharp
//using TwitchIRC;

string nick = "KyleTheScientist";
string auth = "oauth:******************************";
string channel = "RufusMckain"

//Initializes and connects the listener to the server
ChatListener chatListener = new ChatListener(nick, auth, channel); 

//The OnChatMessage event gets called whenever a public chat message is read and relays the message, the user who sent it, and the channel it was sent in.
chatListener.OnChatMessage += (string user, string message, string channel) => 
{
    Console.WriteLine($"{user} said '{message}' in {channel}'s channel");
};

//The OnChatMessage event gets called whenever a message is sent to the IRC server, and relays the raw message data.
chatListener.OnRawIrcMessage += (string message) =>
{
    Console.WriteLine(message);
};


if(chatListener.Connect())
    chatListener.StartListening();
```

You can get your oAuth code [here](https://twitchapps.com/tmi/).

If you want to stop listening:

```csharp
chatListener.StopListening();
```

To send messages via IRC:

```csharp
IrcClient irc = chatListener.Irc;
irc.SendChatMessage("Hello world!"); //Automatically formats text to send as a public chat message
irc.SendIrcMessage("CAP REQ :twitch.tv/membership"); //Sends the raw text to the server
```
