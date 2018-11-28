using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace TwitchIRC
{
    /// <summary>
    /// Utility class for retrieving IRC messages
    /// </summary>
    public class IrcClient
    {
        private string userName;
        private string channel;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        /// <summary>
        /// Creates a new IRC client object and attempts to join the server as the specified user
        /// </summary>
        public IrcClient(string ip, int port, string userName, string password)
        {
            try
            {
                this.userName = userName;

                //Connect to Twitch and create output/input streams
                tcpClient = new TcpClient(ip, port);
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                // Try to join the room
                SendIrcMessage("PASS " + password);
                SendIrcMessage("NICK " + userName);
            }
            catch (Exception e)
            {
                Error.Log(e);
            }
        }

        /// <summary>
        /// Attempts to join the channel
        /// </summary>
        /// <param name="channel">Name of the channel to join</param>
        public void JoinChannel(string channel)
        {
            this.channel = channel;

            //Lets Twitch know what channel we want to read from.
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();

            SendIrcMessage("PING irc.twitch.tv");
        }
        
        /// <summary>
        /// Sends a raw IRC message to the server
        /// </summary>
        public void SendIrcMessage(string message)
        {
            try
            {
                outputStream.WriteLine(message + "\n");
                outputStream.Flush();
            }
            catch (Exception e)
            {
                Error.Log(e);
            }
        }

        /// <summary>
        /// Sends a public message to the chat 
        /// </summary>
        public void SendChatMessage(string message)
        {
            try
            {
                //Format the message and send it
                SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
            }
            catch (Exception e)
            {
                Error.Log(e);
            }
        }

        /// <summary>
        /// Attempts to read a message from the IRC server
        /// </summary>
        /// <returns>Returns a message if one is found.
        /// Returns null if no data is found.</returns>
        public string ReadMessage()
        {
            try
            {
                //Only attempt a read if there is data available, otherwise it will hang
                if (tcpClient.GetStream().DataAvailable)
                {
                    return inputStream.ReadLine(); //reads the stream until the next newline char
                }
            }
            catch (Exception e)
            {
                Error.Log(e);
            }

            return null;
        }

        /// <summary>
        /// Closes the input and output streams
        /// </summary>
        public void Close()
        {
            inputStream.Close();
            outputStream.Close();
        }

        /// <summary>
        /// Returns whether the TCP client object is connected to the IP
        /// </summary>
        public bool Connected
        {
            get
            {
                return tcpClient.Connected;
            }
        }
    }
}
