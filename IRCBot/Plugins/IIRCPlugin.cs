
namespace IRCBot.Plugins
{
	public interface IIRCPlugin
	{
		string Name { get; }

		void Initialize(IIRCBot connection);
	}
}
