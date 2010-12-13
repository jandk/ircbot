
using System;
using IRC;

namespace IRCBot.Plugins
{
	public interface IIRCPlugin
	{
		string Name { get; }

		bool Initialize(IIRCBot connection);
	}
}
