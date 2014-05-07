
using System;

using IRC;
using Parsing.Arithmetic;

namespace IRCBot.Plugins
{
	class CalculatorPlugin
		: IRCPluginBase
	{

		protected override void Initialize()
		{
			Bot.SubscribeToMessage(@"^=", HandleCalculation);
		}

		protected void HandleCalculation(IRCMessage message)
		{
			string expression = message.Message.Substring(1);

			try
			{
				var value = MathInterpreter.InterpretSingle(expression);

				Bot.SendChannelMessage(
					message.Channel,
					String.Format("Result: {0}", value)
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
