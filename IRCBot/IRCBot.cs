
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using IRC;
using IRCBot.Plugins;
using IRCBot.Tools;

namespace IRCBot
{
	class IRCBot
		: IRCConnection, IIRCBot
	{

		List<IIRCPlugin> _plugins;

		Dictionary<String, Action<IRCMessage>> _subscriptions
			= new Dictionary<String, Action<IRCMessage>>();

		RingBuffer<IRCMessage> _buffer
			= new RingBuffer<IRCMessage>(100);


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

		public IList<IIRCPlugin> Plugins
		{
			get { return _plugins; }
		}

		public void SubscribeToMessage(string trigger, Action<IRCMessage> callback)
		{
			if (!_subscriptions.ContainsKey(trigger))
				_subscriptions.Add(trigger, callback);
		}

		public IEnumerable<IRCMessage> MessagesByUser(string user)
		{
			return _buffer.IterateReverse().Where(m => m.User == user);
		}

		#endregion

		void OnMessageReceived(object sender, IRCCommandEventArgs e)
		{
			if (e.Message.Command != "PRIVMSG")
				return;

			if (e.Message.Message.StartsWith("\u0001"))
				return;

			_buffer.Write(e.Message);

			var matchedCallbacks = from subscription in _subscriptions
								   where Regex.IsMatch(e.Message.Message, subscription.Key)
								   select subscription.Value;

			foreach (var callback in matchedCallbacks)
				callback(e.Message);
		}
	}
}
