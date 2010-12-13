
using System;
using IRC;
using System.Collections.Generic;

namespace IRCBot.Plugins
{
	public class MetaPlugin
		: IRCPluginBase
	{
		protected IList<IIRCPlugin> _plugins;

		protected override bool Initialize()
		{
			Connection.SubscribeToMessage("!plugin .+", HandleMessage);

			return true;
		}

		protected void HandleMessage(IRCMessage message)
		{
		}
	}
}
