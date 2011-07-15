
using System;
using System.IO;

using IRC;
using Parsing.Arithmetic;

namespace IRCBot.Plugins
{
	class CalculatorPlugin
		: IRCPluginBase
	{

		static readonly Scanner Scanner = new Scanner();
		static readonly Parser Parser = new Parser();

		protected override bool Initialize()
		{
			Bot.SubscribeToMessage("^=", HandleCalculation);

			return true;
		}

		protected void HandleCalculation(IRCMessage message)
		{
			string formula = message.Message.Substring(1);
			double result = 0;

			try
			{
				using (var reader = new StringReader(formula))
					result = (double)Parser.Parse(Scanner.Scan(reader));

				Bot.SendChannelMessage(
					message.Channel,
					String.Format("Result: {0}", result)
				);
			}
			catch (Exception ex)
			{
				Bot.SendChannelMessage(
					message.Channel,
					"Error: " + ex.Message
				);

				return;
			}
		}
	}
}
