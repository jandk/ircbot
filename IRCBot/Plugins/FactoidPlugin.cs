
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using IRC;
using IRCBot.Tools;

namespace IRCBot.Plugins
{
	public class FactoidPlugin
		: IRCPluginBase
	{
		private Dictionary<String, List<String>> _factoids
			= new Dictionary<string, List<string>>();

		protected override bool Initialize()
		{
			if (Data == null)
				return false;

			Bot.SubscribeToMessage("^!factoid", HandleMessage);

			ReadAllFactoids();

			return true;
		}

		private void ReadAllFactoids()
		{
			using (TextReader reader = new StreamReader(Data))
			{
				_factoids = (from factoid in reader.ReadLines()
							 let split = factoid.IndexOf(';')
							 let user = factoid.Substring(0, split)
							 let fact = factoid.Substring(split)
							 group factoid by user into factoidsPerUser
							 select factoidsPerUser
							).ToDictionary(x => x.Key, x => x.ToList());
			}
		}

		protected void HandleMessage(IRCMessage message)
		{
		}
	}
}
