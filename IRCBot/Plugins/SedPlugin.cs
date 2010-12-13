
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using IRC;

namespace IRCBot.Plugins
{
	class SedPlugin
		: IRCPluginBase
	{
		// TODO: Match modifiers iI and g and number
		const int NumberOfMessages = 5;
		static readonly Regex SedRegex = new Regex(@"^([ds])/((?:\\/|[^/])+)/(?:((?:\\/|[^/])*)/)?([gi]+)?(?:\s+(\d+))?$");
		static readonly Regex RepRegex = new Regex(@"\\\d");

		protected Dictionary<string, List<string>> _history
			= new Dictionary<string, List<string>>();

		protected override bool Initialize()
		{
			Connection.SubscribeToMessage(".+", HandleMessage);

			return true;
		}

		void HandleMessage(IRCMessage message)
		{
			if (SedRegex.IsMatch(message.Message))
				SearchReplace(message);
			else
				AddSentence(message);
		}

		private void AddSentence(IRCMessage message)
		{
			if (!_history.ContainsKey(message.User))
				_history.Add(message.User, new List<string>(NumberOfMessages));

			var list = _history[message.User];

			if (list.Count == NumberOfMessages)
			{
				for (int i = 0; i < NumberOfMessages - 1; i++)
					list[i] = list[i + 1];

				list.RemoveAt(NumberOfMessages - 1);
			}

			list.Add(message.Message);
		}

		private void SearchReplace(IRCMessage message)
		{
			// Weust` found this bug,
			if (!_history.ContainsKey(message.User))
				return;

			var match = SedRegex.Match(message.Message);

			// 1: Check the modifiers
			bool global = false;
			bool caseInsensitive = false;

			string modifier = match.Groups[4].Value;
			if (modifier.Contains("g"))
				global = true;
			if (modifier.Contains("i") || modifier.Contains("I"))
				caseInsensitive = true;

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
			int offset = 1;
			if (match.Groups[5].Success)
				offset = Convert.ToInt32(match.Groups[5].Value);

			var list = _history[message.User];
			if (offset > list.Count)
				return;

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

			string msg = list[list.Count - offset];
			if (regex.IsMatch(msg))
			{
				// Send the edited message
				Connection.SendChannelMessage(
					message.Channel,
					String.Format(
						"{0} meant: {1}",
						message.User,
						regex.Replace(msg, replacement, global ? Int32.MaxValue : 1)
					)
				);
			}
		}
	}
}
