
using System;
using System.Collections.Specialized;
using System.IO;

namespace IRCBot
{
	class Program
	{
		// TODO: Better location
		static string Config = "config.txt";

		static void Main(string[] args)
		{
			var config = ReadConfig();

			using (IRCBot bot = new IRCBot(config["host"], Convert.ToUInt16(config["port"]), config["nick"]))
			{
				bot.Join(config["channel"]);

				Console.WriteLine("Press q to quit...");
				while (Console.ReadKey(true).KeyChar != 'q')
					;
			}
		}

		static StringDictionary ReadConfig()
		{
			if (!Directory.Exists(Config))
				throw new Exception("Config file not found.");

			StringDictionary config = new StringDictionary();

			string[] lines = File.ReadAllLines(Config);
			foreach (var line in lines)
			{
				if (line.IndexOf('=') <= 0)
					continue;

				var split = line.Split(new char[] { '=' }, 2);

				if (!config.ContainsKey(split[0].Trim()))
					config.Add(split[0].Trim(), split[1].Trim());
			}

			return config;
		}
	}
}
