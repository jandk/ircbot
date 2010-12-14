
using System;
using System.Collections.Generic;
using System.Text;

namespace IRC
{
	public class IRCMessage
	{
		public static readonly IRCMessage Empty = new IRCMessage();

		public string Server { get; set; }
		public string Nick { get; set; }
		public string User { get; set; }
		public string Host { get; set; }
		public string Command { get; set; }
		public string Message { get; set; }
		public List<string> Params { get; set; }

		private string _message;
		private int _index;
		private int _lastIndex;

		public string Channel
		{
			get
			{
				if (Command == "PRIVMSG" && Params[0].StartsWith("#"))
					return Params[0].Substring(1);

				return String.Empty;
			}
			set
			{
				if (Command == "PRIVMSG" && Params[0].StartsWith("#"))
					Params[0] = "#" + value;
			}
		}

		public IRCMessage()
		{
			Params = new List<string>();
		}

		public IRCMessage(string rawMessage)
			: this()
		{
			if (rawMessage == null)
				throw new ArgumentNullException("rawMessage");

			_message = rawMessage;
			_index = 0;
		}

		public static IRCMessage FromString(string rawMessage)
		{
			if (rawMessage.Length == 0)
				return Empty;

			var message = new IRCMessage(rawMessage);
			message.Parse();
			return message;
		}

		#region Parsing

		public void Parse()
		{
			// <message>  ::= [':' <prefix> <SPACE> ] <command> <params> <crlf>

			string token = GetNextToken();

			if (token.StartsWith(":"))
			{
				// <prefix>   ::= <servername> | <nick> [ '!' <user> ] [ '@' <host> ]

				// In this case there is a server
				int idxEx = token.IndexOf('!');
				int idxAt = token.IndexOf('@');

				if (idxEx > 0 && idxAt > idxEx)
				{

					// In this case there is a nick, optionally a user and a host
					if (idxEx > 0)
					{
						Nick = token.Substring(1, idxEx - 1);

						if (idxAt > idxEx)
						{
							User = token.Substring(idxEx + 1, idxAt - idxEx - 1);
							Host = token.Substring(idxAt + 1);
						}
						else User = token.Substring(idxEx + 1);
					}
					else Nick = token.Substring(1);
				}
				else Server = token.Substring(1);

				Command = GetNextToken();
			}
			else Command = token;

			while ((token = GetNextToken()).Length > 0)
			{
				if (token.StartsWith(":"))
				{
					Message = _message.Substring(_lastIndex + 1);
					return;
				}

				Params.Add(token);
			}
		}

		private string GetNextToken()
		{
			_lastIndex = _index;
			int tempIndex = _index;

			while (tempIndex < _message.Length
				&& _message[tempIndex] != '\x20'
				&& _message[tempIndex] != '\x00'
			) tempIndex++;

			if (tempIndex > _message.Length)
			{
				// At the end, so clear
				_index = 0;
				_lastIndex = 0;
				_message = null;
				return String.Empty;
			}

			string token = _message.Substring(_index, tempIndex - _index);
			_index = tempIndex + 1; // Skip the space
			return token;
		}

		#endregion

		#region Object Members

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			// Add the prefix
			bool hasServer = !String.IsNullOrEmpty(Server);
			bool hasNick = !String.IsNullOrEmpty(Nick);

			if (hasServer && hasNick)
				throw new InvalidOperationException("Cannot have a server and a nick!user@host at the same time.");

			if (hasServer)
				sb.Append(":" + Server + " ");

			if (hasNick)
			{
				sb.Append(":" + Nick + " ");

				if (!String.IsNullOrEmpty(User))
					sb.Append("!" + User + " ");

				if (!String.IsNullOrEmpty(Host))
					sb.Append("@" + Host + " ");
			}

			// Add the command
			sb.Append(Command + " ");

			// Add the parameters
			foreach (var param in Params)
				sb.Append(param + " ");

			if (!String.IsNullOrEmpty(Message))
				sb.Append(":" + Message + " ");

			return sb.ToString(0, sb.Length - 1);
		}

		#endregion

	}
}
