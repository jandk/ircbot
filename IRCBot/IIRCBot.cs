
using System;

namespace IRC
{
    public interface IIRCBot
    {
        void SendMessage(IRCMessage message);
        void SubscribeToTrigger(string trigger, Action<IRCMessage> CallBack);
    }
}
