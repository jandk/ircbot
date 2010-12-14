
using System;
using System.IO;

namespace IRCBot.Plugins
{
	public abstract class IRCPluginBase
		: IIRCPlugin, IDisposable
	{
		const string BasePath = ".";

		public IIRCBot Bot { get; private set; }
		public string Name { get; protected set; }
		public Stream Data { get; private set; }

		public bool Initialize(IIRCBot bot)
		{
			Bot = bot;
			Name = GetType().Name;

			return Initialize();
		}

		protected abstract bool Initialize();

		#region IDisposable Members

		public void Dispose()
		{
			Data.Dispose();
		}

		#endregion
	}
}
