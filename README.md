# TwitchIRC
An IRC class library for C#

This is a library I made to help me work Twitch integration into Unity games, but it works for any C# application.

### Prerequisites

.NET framework 2.0 or higher

## Usage

Here's a quick example of a console application that simply relays chat messages until the program is closed: 

```csharp
string nick = "KyleTheScientist";
string auth = "oauth:******************************";
string channel = "RufusMckain"

ChatListener chatListener = new ChatListener(nick, auth, channel); //Initializes and connect the listener to the server

//The OnChatMessage event gets called whenever a public chat message is read
chatListener.OnChatMessage += (string user, string message, string channel) => 
{
    Console.WriteLine($"{user} said '{message}' in {channel}'s channel");
};

//The OnChatMessage event gets called whenever a message is sent to the IRC server
chatListener.OnRawIrcMessage += (string message) =>
{
    Console.WriteLine(message);
};

chatListener.StartListening();
```
