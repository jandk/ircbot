
using System;
using System.IO;

namespace IRCBot.Plugins
{
	public abstract class IRCPluginBase
		: IIRCPlugin
	{
		public IIRCBot Bot { get; private set; }
		public string Name { get; protected set; }

		public void Initialize(IIRCBot bot)
		{
			Bot = bot;
			Name = GetType().Name;

			Initialize();
		}

		protected abstract void Initialize();
	}
}
