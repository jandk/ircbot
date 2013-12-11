
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

		protected override void Initialize()
		{
			Bot.SubscribeToMessage(@"^= *[a-z0-9(]", HandleCalculation);
		}

		protected void HandleCalculation(IRCMessage message)
		{
			string formula = message.Message.Substring(1);

			try
			{
				double result;
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
			}
		}
	}
}
