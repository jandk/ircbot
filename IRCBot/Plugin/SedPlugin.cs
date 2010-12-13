
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
        static readonly Regex SedRegex = new Regex(@"s/((?:(?<=\\)/|[^/])+)/((?:(?<=\\)/|[^/])+)/?");
        static readonly Regex RepRegex = new Regex(@"\\\d");

        protected Dictionary<string, List<string>> _history
            = new Dictionary<string, List<string>>();

        protected override bool Initialize()
        {
            Name = GetType().Name;

            Connection.SubscribeToTrigger(".+", HandleMessage);

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
            if (!_history.ContainsKey(message.Nick))
                _history.Add(message.Nick, new List<string>(NumberOfMessages));

            var list = _history[message.Nick];

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
            if (!_history.ContainsKey(message.Nick))
                return;

            var match = SedRegex.Match(message.Message);
            Regex regex;

            try { regex = new Regex(match.Groups[1].Value); }
            catch (ArgumentException) { return; }

            string replacement = RepRegex.Replace(match.Groups[2].Value, "$$1");

            var list = _history[message.Nick];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (regex.IsMatch(list[i]))
                {
                    SendMessageToChannel(
                        message.Channel,
                        String.Format(
                            "{0} meant: {1}",
                            message.Nick,
                            regex.Replace(list[i], replacement)
                        )
                    );
                    return;
                }
            }
        }
    }
}
