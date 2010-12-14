
using System;
using System.Linq;
using System.Text.RegularExpressions;

using IRC;

namespace IRCBot.Plugins
{
	class SedPlugin
		: IRCPluginBase
	{
		// TODO: Match modifiers iI and g and number
		const int NumberOfMessages = 5;
		const string Sed = @"^([ds])/((?:\\/|[^/])+)/(?:((?:\\/|[^/])*)/)?([gi]+)?(?:\s+(\d+))?$";

		static readonly Regex SedRegex = new Regex(Sed);
		static readonly Regex RepRegex = new Regex(@"\\\d");

		protected override bool Initialize()
		{
			Bot.SubscribeToMessage(Sed, HandleMessage);

			return true;
		}

		private void HandleMessage(IRCMessage message)
		{
			// Weust` found this bug,
			if (!Bot.MessagesByUser(message.User).Any(m => !SedRegex.IsMatch(m.Message)))
				return;

			var match = SedRegex.Match(message.Message);

			// 1: Check the modifiers
			bool global = false;
			bool caseInsensitive = false;

			if (match.Groups[4].Success)
			{
				string modifier = match.Groups[4].Value;

				if (modifier.Contains("g"))
					global = true;
				if (modifier.Contains("i") || modifier.Contains("I"))
					caseInsensitive = true;
			}

			// 2: Check if the regex is valid.
			Regex regex;
			try
			{
				regex = new Regex(
					match.Groups[2].Value,
					caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None
				);
			}
			catch (ArgumentException) { return; }

			// 3: Check for the message offset
			int offset = 0;
			if (match.Groups[5].Success)
				offset = Convert.ToInt32(match.Groups[5].Value) - 1;

			// 4: Check what mode we're in
			string mode = match.Groups[1].Value;

			string replacement = String.Empty;
			if (mode == "d")
			{
				if (match.Groups[3].Success)
					return;
			}
			if (mode == "s")
			{
				if (!match.Groups[3].Success)
					return;

				replacement = match.Groups[3].Value;
				replacement = replacement.Replace("$", @"\$");
				replacement = RepRegex.Replace(replacement, "$$1");
			}

			var msg = Bot.MessagesByUser(message.User).Where(m => !SedRegex.IsMatch(m.Message))
													  .Skip(offset)
													  .FirstOrDefault();

			if (msg != null && regex.IsMatch(msg.Message))
			{
				// Send the edited message
				Bot.SendChannelMessage(
					message.Channel,
					String.Format(
						"{0} meant: {1}",
						message.Nick,
						regex.Replace(msg.Message, replacement, global ? Int32.MaxValue : 1)
					)
				);
			}
		}
	}
}
