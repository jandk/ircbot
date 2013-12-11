
using System.IO;

namespace IRCBot.Plugins
{
	public interface IIRCPlugin
	{
		string Name { get; }
		Stream Data { get; }

		void Initialize(IIRCBot connection);
	}
}
