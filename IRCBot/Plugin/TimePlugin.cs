
using System;
using IRC;

namespace IRCBot.Plugins
{
    class TimePlugin
        : IRCPluginBase
    {
        protected override bool Initialize()
        {
            Name = GetType().Name;

            Connection.SubscribeToTrigger("!time", HandleMessage);

            return true;
        }

        protected void HandleMessage(IRCMessage message)
        {
            SendMessageToChannel(
                message.Channel,
                String.Format("The time is now {0}.", DateTime.Now.ToShortTimeString())
            );
        }
    }
}
