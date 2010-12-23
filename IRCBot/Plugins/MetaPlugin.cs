
using System;

using IRC;
using System.Reflection;
using System.Collections.Generic;

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
			List<string> pluginList = this.GetAllClasses("IRCBot.Plugins");
			string answer = "Following plugins are currently loaded:";

			foreach(string pluginName in pluginList)
				answer += ", '"+pluginName+"'";

			answer += ".";

			Bot.SendChannelMessage(message.Channel, answer);
			//Console.WriteLine("pluginlist: "+pluginName);
		}

		/// <summary>
		/// Method to populate a list with all the class
		/// in the namespace provided by the user
		/// </summary>
		/// <param name="nameSpace">The namespace the user wants searched</param>
		/// <returns></returns>
		static List<string> GetAllClasses(string nameSpace)
		{
		    //create an Assembly and use its GetExecutingAssembly Method
		    //http://msdn2.microsoft.com/en-us/library/system.reflection.assembly.getexecutingassembly.aspx
		    Assembly asm = Assembly.GetExecutingAssembly();
		    //create a list for the namespaces
		    List<string> namespaceList = new List<string>();
		    //create a list that will hold all the classes
		    //the suplied namespace is executing
		    List<string> returnList = new List<string>();
		    //loop through all the "Types" in the Assembly using
		    //the GetType method:
		    //http://msdn2.microsoft.com/en-us/library/system.reflection.assembly.gettypes.aspx
		    foreach (Type type in asm.GetTypes())
		    {
		        if (type.Namespace == nameSpace)
		            namespaceList.Add(type.Name);
		    }
		    //now loop through all the classes returned above and add
		    //them to our classesName list
		    foreach (String className in namespaceList)
		        returnList.Add(className);
		    //return the list
		    return returnList;
		}
	}
}
