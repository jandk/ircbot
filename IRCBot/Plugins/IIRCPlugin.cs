
using System;
using System.IO;

using IRC;

namespace IRCBot.Plugins
{
	public interface IIRCPlugin
	{
		string Name { get; }
		Stream Data { get; }

		bool Initialize(IIRCBot connection);
	}
}
