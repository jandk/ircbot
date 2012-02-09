
using System;
using System.IO;
using System.Collections.Generic;


using IRC;

namespace IRCBot.Plugins
{
	class InternetMeme
		: IRCPluginBase
	{
		private Dictionary<string, string> memes;


		protected override bool Initialize()
		{
			//Parses memesFile into this class's memes dictionary and subscribes triggers
			ParseAndSubscribeMemesFile();

			Bot.SubscribeToMessage("^!reload memesFile$", HandleReloadmemesFile);
			
			return true;
		}


		protected void ParseAndSubscribeMemesFile()
		{
			memes = new Dictionary<string, string>();
			string memesFile = Config.Instance["memesFile"];

			if (!File.Exists(memesFile))
				throw new Exception("memesFile '"+memesFile+"' does not exist");

			string[] lines = File.ReadAllLines(memesFile);
			foreach(string line in lines)
			{
				if(line.IndexOf('\t') <=0 || line.StartsWith("#")) //Lines must have a tab and should not start with a #
					continue;

				string[] split = line.Split(new char[] {'\t'}, 2);
				string trigger = split[0].Trim().Replace("__BOTNICK__", Config.Instance["nick"]);
				string response = split[1].Trim();

				if(!memes.ContainsKey(trigger))
					memes.Add(trigger, response);

				// Subscribe to every trigger in the memes dictionary
				foreach(KeyValuePair<string, string> pair in memes)
					Bot.SubscribeToMessage(pair.Key + "$", HandleResponse);
			}
		}


		protected void HandleResponse(IRCMessage message)
		{
			if (memes.ContainsKey("^" + message.Message))
				Bot.SendChannelMessage(
					message.Channel,
					String.Format( memes["^"+message.Message].Replace("__USER__", message.Nick) )
				);
		}


		// Made a function for this so we could later attach it to an bot_nickchange event
		// so the __BOTNICK__ vars will be correctly replaced
		protected void HandleReloadmemesFile(IRCMessage message)
		{
			
			// Old (current) subscriptions should be unsubscribed first...
			foreach(KeyValuePair<string, string> pair in memes)
				Bot.UnsubscribeFromMessage(pair.Key + "$");

			ParseAndSubscribeMemesFile();
		}
	}
}