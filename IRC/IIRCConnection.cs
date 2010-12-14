
using System;

namespace IRC
{
	public interface IIRCConnection
	{
		void SendRawMessage(IRCMessage message);
		void SendChannelMessage(string channel, string message);
	}
}
