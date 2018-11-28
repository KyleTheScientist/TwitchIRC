using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TwitchIRC
{
    /// <summary>
    /// Listens to IRC channels for messages and calls OnRawIrcMessage/OnChatMessage when 
    /// a new message is found
    /// </summary>
    public class ChatListener
    {
        private const string PRIVATE_STRING = "PRIVMSG";

        private string nick, oauth, channel;
        private bool listening;

        private IrcClient irc;
        private Thread readThread;

        /// <summary></summary>
        public delegate void IrcDelegate(string message);

        /// <summary></summary>
        public delegate void ChatDelegate(string user, string message, string channel);

        /// <summary>Gets called whenever a new message gets sent to the server</summary>
        public event IrcDelegate OnRawIrcMessage;

        /// <summary>Gets called whenever a new PRIVMSG gets sent to the server</summary>
        public event ChatDelegate OnChatMessage;

        /// <summary>Creates a new instance of ChatListener</summary>
        /// <param name="nick">The account's username</param>
        /// <param name="oauth">The account's oauth code. Obtainable from https://twitchapps.com/tmi/ </param>
        /// <param name="channel">The channel the bot should join. </param>
        public ChatListener(string nick, string oauth, string channel)
        {
            this.nick = nick;
            this.oauth = oauth;
            this.channel = channel.ToLower();
            readThread = new Thread(new ThreadStart(Listen)); //initialize thread
        }

        /// <summary>
        /// Attenmpts to connect to the Twitch server and joins the set channel. 
        /// Generates a new IRC Client object
        /// </summary>
        /// <returns>Returns whether the connection was successful</returns>
        public bool Connect()
        {
            try
            {
                irc = new IrcClient("irc.twitch.tv", 6667, nick, oauth); //establish connection
                irc.JoinChannel(this.channel);
                return irc.Connected;
            }
            catch (Exception e)
            {
                Error.Log(e);
            }
            return false;
        }

        //Continuously 
        private void Listen()
        {
            //irc messages come in the format ":nickname!username@nickname.tmi.twitch.tv PRIVMSG #channelName :message"
            while (listening)
            {
                if (irc.Connected)
                {
                    string ircMessage = irc.ReadMessage();
                    if (ircMessage == null) continue;

                    if (OnRawIrcMessage != null)
                        OnRawIrcMessage(ircMessage);

                    if (ircMessage.Contains(PRIVATE_STRING))
                    {
                        string nickname = ircMessage.Substring(1, ircMessage.IndexOf('!') - 1); //get nickname from message

                        //get message text
                        string channelAndText = SplitAtFirst(PRIVATE_STRING, ircMessage)[1]; //get chars after the PRIVMSG keyword
                        string message = SplitAtFirst(":", channelAndText)[1]; //gets chars after the ':' 

                        if (OnChatMessage != null)
                            OnChatMessage(nickname, message, channel);
                    }
                }
            }
        }

        ///<summary>
        ///Splits the string into two at the index of the key, returns an array containing the two halves of the string.
        ///Returns null if the key was not contained in the string
        ///Returned strings do not contain the key
        ///</summary>
        private string[] SplitAtFirst(string key, string s)
        {
            int privIndex = s.IndexOf(key);
            if (privIndex == -1) return null;

            String first = s.Substring(0, privIndex).Trim();
            String last = s.Substring(privIndex + key.Length).Trim();
            string[] result = new string[] { first, last };
            return result;
        }

        /// <summary>
        /// Starts the listening thread. 
        /// </summary>
        public void StartListening()
        {
            readThread.Start();
            listening = true;
        }

        /// <summary>
        /// Safely stops the listening thread. 
        /// </summary>
        public void StopListening()
        {
            listening = false;
        }

        /// <summary>
        /// Dangerously stops the listening thread. 
        /// </summary>
        public void ForceStopListening()
        {
            listening = false;
            readThread.Abort();
        }

        /// <summary></summary>
        public IrcClient Irc
        {
            get
            {
                return irc;
            }
            set
            {
                this.irc = value;
            }
        }

        /// <summary>
        /// Returns whether or not the irc client is connected to Twitch
        /// </summary>
        public bool Connected
        {
            get
            {
                return irc.Connected;
            }
        }
    }
}
