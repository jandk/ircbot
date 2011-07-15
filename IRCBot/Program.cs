
using System;
using System.Collections.Specialized;
using System.IO;

namespace IRCBot
{
	class Program
	{

		// TODO: Better location
		static string ConfigFile = "config.txt";

		static void Main(string[] args)
		{
			Config.Instance.Initialize(ConfigFile);

			string host = Config.Instance["host"];
			ushort port = Convert.ToUInt16(Config.Instance["port"]);
			string nick = Config.Instance["nick"];

			using (IRCBot bot = new IRCBot(host, port, nick))
			{
				bot.Join(Config.Instance["channel"]);

				Console.WriteLine("Press q to quit...");
				while (Console.ReadKey(true).KeyChar != 'q')
					;
			}
		}

	}
}
