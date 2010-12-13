
using System;
using IRC;

namespace IRCBot.Plugins
{
	class TimePlugin
		: IRCPluginBase
	{
		protected override bool Initialize()
		{
			Connection.SubscribeToMessage("!time", HandleMessage);

			return true;
		}

		protected void HandleMessage(IRCMessage message)
		{
			Connection.SendChannelMessage(
				message.Channel,
				String.Format("The time is now {0}.", DateTime.Now.ToShortTimeString())
			);
		}
	}
}
