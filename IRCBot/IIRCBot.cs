
using System;
using System.Collections.Generic;

using IRC;
using IRCBot.Plugins;

namespace IRCBot
{
	public interface IIRCBot
		: IIRCConnection
	{
		IList<IIRCPlugin> Plugins { get; }

		IEnumerable<IRCMessage> MessagesByUser(string user);
		void SubscribeToMessage(string regex, Action<IRCMessage> CallBack);
	}
}
