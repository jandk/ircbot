
using System;
using IRC;

namespace IRCBot.Plugins
{
	public abstract class IRCPluginBase
		: IIRCPlugin
	{
		public IIRCBot Connection { get; protected set; }
		public string Name { get; protected set; }

		public bool Initialize(IIRCBot connection)
		{
			Connection = connection;
			Name = GetType().Name;

			return Initialize();
		}

		protected abstract bool Initialize();
	}
}
