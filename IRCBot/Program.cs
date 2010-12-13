
using System;
using System.Collections.Generic;
using IRC;

namespace IRCBot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (IRCBot bot = new IRCBot("irc.freenode.net", 6667, "tjoenbot"))
            {
                bot.Join("heleos");

                Console.WriteLine("Press q to quit...");
                while (Console.ReadKey(true).KeyChar != 'q')
                    ;
            }

            //const string rawMessage = ":tjoener!~tjoener@vm.tjoener.be PRIVMSG #heleos :dit is nen test";
            //IRCMessage mess = IRCMessage.FromString(rawMessage);
            //Console.WriteLine(rawMessage == mess.ToString());

            //var messages = from line in File.ReadAllLines("telnet.log")
            //               let parsed = IRCMessage.FromString(line)
            //               select new
            //               {
            //                   Equals = parsed.ToString() == line,
            //                   OldMessage = line,
            //                   NewMessage = parsed
            //               };

            //var allMessages = messages.Where(x => !x.Equals).ToArray();
        }
    }
}
