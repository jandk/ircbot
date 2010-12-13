
using System;
using System.Collections.Generic;

using IRCBot.Plugins;

namespace IRC
{
	public interface IIRCBot
		: IIRCConnection
	{
		IList<IIRCPlugin> Plugins { get; }

		void SubscribeToMessage(string regex, Action<IRCMessage> CallBack);
	}
}
