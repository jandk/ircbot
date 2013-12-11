
using System;

using IRC;

namespace IRCBot.Plugins
{
	class TimePlugin
		: IRCPluginBase
	{
		protected override void Initialize()
		{
			Bot.SubscribeToMessage("^!time$", HandleTimeMessage);
			Bot.SubscribeToMessage("^!date$", HandleDateMessage);
		}

		protected void HandleTimeMessage(IRCMessage message)
		{
			Bot.SendChannelMessage(
				message.Channel,
				String.Format("The time is now {0}.", DateTime.Now.ToShortTimeString())
			);
		}

		protected void HandleDateMessage(IRCMessage message)
		{
			Bot.SendChannelMessage(
				message.Channel,
				String.Format("The date is now {0}.", DateTime.Now.ToShortDateString())
			);
		}
	}
}
