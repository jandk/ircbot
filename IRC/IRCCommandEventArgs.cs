
using System;

namespace IRC
{
	public class IRCCommandEventArgs
		: EventArgs
	{
		public IRCMessage Message { get; set; }

		public IRCCommandEventArgs(IRCMessage message)
		{
			Message = message;
		}
	}
}
