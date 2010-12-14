
using System;

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
		}
	}
}
