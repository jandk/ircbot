
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using IRC;

namespace IRCBot.Plugins
{
	public class MetaPlugin
		: IRCPluginBase
	{
		protected override bool Initialize()
		{
			Bot.SubscribeToMessage("^!plugin", HandleMessage);

			return true;
		}

		protected void HandleMessage(IRCMessage message)
		{
			string[] splittedString = message.Message.Split(' ');

			if (splittedString.GetLength(0) > 1)
			{
				switch (splittedString[1])
				{
					case "list":
						HandleMessageList(message);
						break;
					case "help":
						Bot.SendChannelMessage(message.Channel, "Help for !plugin that still needs to be written.");
						Bot.SendChannelMessage(message.Channel, "!plugin help - Shows help (this message)");
						Bot.SendChannelMessage(message.Channel, "!plugin list - Shows list of loaded plugins");
						break;
					default:
						Bot.SendChannelMessage(message.Channel, "Unknown command, use !plugin help to see the help.");
						break;
				}
			}
			else
				Bot.SendChannelMessage(message.Channel, "Please provide a command, use !plugin help to see the help.");
		}


		protected void HandleMessageList(IRCMessage message)
		{
			IEnumerable<string> pluginList = GetAllClasses("IRCBot.Plugins");

			string answer = pluginList.Aggregate(
				"Following plugins are currently loaded:",
				(current, pluginName) => current + (", '" + pluginName + "'")
			) + ".";

			Bot.SendChannelMessage(message.Channel, answer);
		}

		/// <summary>
		/// Method to populate a list with all the class
		/// in the namespace provided by the user
		/// </summary>
		/// <param name="nameSpace">The namespace the user wants searched</param>
		/// <returns></returns>
		static IEnumerable<string> GetAllClasses(string nameSpace)
		{
			return (from type in Assembly.GetExecutingAssembly().GetTypes()
					where type.Namespace == nameSpace
					select type.Name).ToList();
		}
	}
}
