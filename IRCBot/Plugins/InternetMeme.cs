
using System;

using IRC;

namespace IRCBot.Plugins
{
	class InternetMeme
		: IRCPluginBase
	{
		protected override bool Initialize()
		{
			Bot.SubscribeToMessage("^orly", HandleORLYMessage);
			Bot.SubscribeToMessage("^ORLY", HandleORLYMessage);
			
			return true;
		}

		protected void HandleORLYMessage(IRCMessage message)
		{
			Bot.SendChannelMessage(
				message.Channel,
				String.Format("YARLY!")
			);
		}
	}
}
