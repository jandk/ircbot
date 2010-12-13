
using System;
using IRC;
using System.Collections.Generic;

namespace IRCBot.Plugins
{
	public class MetaPlugin
		: IRCPluginBase
	{
		protected override bool Initialize()
		{
			Bot.SubscribeToMessage("^!plugin", HandleMessage);

			return true;
		}

		protected void HandleMessage(IRCMessage message)
		{
		}
	}
}
