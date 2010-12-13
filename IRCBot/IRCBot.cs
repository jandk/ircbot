
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using IRC;
using IRCBot.Plugins;

namespace IRCBot
{
    class IRCBot
        : IRCConnection, IIRCBot
    {

        #region Subscription

        class Subscription
        {
            public Regex Trigger { get; set; }
            public Action<IRCMessage> Callback { get; set; }
        }

        #endregion

        List<IIRCPlugin> _plugins;
        List<Subscription> _subscriptions
            = new List<Subscription>();


        public IRCBot(string host, ushort port, string nick)
            : base(host, port, nick, true)
        {
            LoadPlugins();

            MessageReceived += OnMessageReceived;
        }

        #region Plugins

        private void LoadPlugins()
        {
            Type[] emptyTypeArray = new Type[0];

            _plugins = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where type.GetInterfaces().Contains(typeof(IIRCPlugin))
                            && type.IsAbstract == false
                            && type.GetConstructor(emptyTypeArray) != null
                        select type.GetConstructor(emptyTypeArray).Invoke(null)
                        ).Cast<IIRCPlugin>().ToList();

            foreach (var plugin in _plugins)
                plugin.Initialize(this);
        }

        #endregion

        #region IIRCBot Members

        public void SubscribeToTrigger(string trigger, Action<IRCMessage> callback)
        {
            if (!_subscriptions.Any(s => s.Trigger.ToString() == trigger))
                _subscriptions.Add(new Subscription()
                {
                    Trigger = new Regex(trigger),
                    Callback = callback
                });
        }

        #endregion

        void OnMessageReceived(object sender, IRCCommandEventArgs e)
        {
            if (e.Message.Command != "PRIVMSG")
                return;

            string message = e.Message.Message.Substring(1);
            var matchedTriggers = _subscriptions.Where(s => s.Trigger.IsMatch(message));

            foreach (var trigger in matchedTriggers)
                trigger.Callback(e.Message);
        }
    }
}
