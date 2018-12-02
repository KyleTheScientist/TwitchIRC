using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

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

        private Stopwatch pingTimer;

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
            pingTimer = new Stopwatch();
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
                    HandlePings();

                    string ircMessage = irc.ReadMessage();
                    if (ircMessage == null) continue;

                    if (ircMessage.StartsWith("PING")) //we need to send back a PONG when we get PING'd so we don't disconnect
                        irc.SendIrcMessage("PONG irc.twitch.tv");

                    if (OnRawIrcMessage != null)
                        OnRawIrcMessage(ircMessage);

                    if (ircMessage.Contains(PRIVATE_STRING))
                    {
                        string nickname = ircMessage.Substring(1, ircMessage.IndexOf('!') - 1); //get nickname from message

                        //get message text
                        string channelAndMessage = SplitAtFirst(PRIVATE_STRING, ircMessage)[1]; //get chars after the PRIVMSG keyword
                        string message = SplitAtFirst(":", channelAndMessage)[1]; //gets chars after the ':' 

                        if (OnChatMessage != null)
                            OnChatMessage(nickname, message, channel);
                    }
                }
            }
        }

        /// <summary>
        /// A ping must be sent to the server every
        /// </summary>
        private void HandlePings()
        {
            if (pingTimer.Elapsed.Seconds > 240) //ping once every 4 minutes
            {
                irc.SendIrcMessage("PING irc.twitch.tv");

                pingTimer.Reset(); //reset timer
                pingTimer.Start();
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
            pingTimer.Start();
            listening = true;
        }

        /// <summary>
        /// Safely stops the listening thread. 
        /// </summary>
        public void StopListening()
        {
            pingTimer.Stop();
            pingTimer.Reset();

            listening = false;
        }

        /// <summary>
        /// Dangerously stops the listening thread. 
        /// </summary>
        public void ForceStopListening()
        {
            pingTimer.Stop();
            pingTimer.Reset();

            readThread.Abort();
            listening = false;
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
                return (irc != null) ? irc.Connected : false;
            }
        }
    }
}
