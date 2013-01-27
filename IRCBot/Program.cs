
using System;

namespace IRCBot
{
	class Program
	{

		// TODO: Better location
		private const string ConfigFile = "config.txt";

		static void Main()
		{
			Config.Instance.Initialize(ConfigFile);

			string host = Config.Instance["host"];
			ushort port = Convert.ToUInt16(Config.Instance["port"]);
			string nick = Config.Instance["nick"];

			using (var bot = new IRCBot(host, port, nick))
			{
				bot.Join(Config.Instance["channel"]);

				Console.WriteLine("Press q to quit...");
				while (Console.ReadKey(true).KeyChar != 'q') { }
			}
		}

	}
}
