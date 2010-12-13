
using System;
using IRC;

namespace IRCBot.Plugins
{
    public abstract class IRCPluginBase
        : IIRCPlugin
    {
        public IIRCBot Connection { get; protected set; }
        public string Name { get; protected set; }

        public bool Initialize(IIRCBot connection)
        {
            Connection = connection;

            return Initialize();
        }

        public void SendMessageToChannel(string channel, string message)
        {
            IRCMessage ircMessage = new IRCMessage();
            ircMessage.Command = "PRIVMSG";
            ircMessage.Params.Add("#" + channel);
            ircMessage.Message = message;

            Connection.SendMessage(ircMessage);
        }

        protected abstract bool Initialize();
    }
}
