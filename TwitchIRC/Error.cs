using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchIRC
{
    public static class Error
    {
        /// <summary>
        /// Logs an exception to the console
        /// </summary>
        public static void Log(Exception e)
        {
            Console.Error.WriteLine(e.Message + "\n" + e.StackTrace);

        }
        
        /// <summary>
        /// Logs an error message to the console
        /// </summary>
        public static void Log(string s)
        {
            Console.Error.WriteLine(s);
        }
    }
}
