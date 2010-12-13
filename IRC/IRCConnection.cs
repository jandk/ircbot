
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace IRC
{
    public class IRCConnection
        : IDisposable
    {

        #region Fields

        private string _host;
        private ushort _port;
        private string _nick;

        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Thread _listenThread;

        private List<string> _channels
             = new List<string>();

        #endregion

        #region Events

        public event EventHandler<IRCCommandEventArgs> MessageReceived;

        public void RaiseMessageReceived(IRCMessage message)
        {
            if (MessageReceived != null)
                MessageReceived(this, new IRCCommandEventArgs(message));
        }

        #endregion

        public IRCConnection(string host, ushort port, string nick, bool connect)
        {
            _host = host;
            _port = port;
            _nick = nick;

            if (connect)
                Connect();
        }

        public void Connect()
        {
            Debug.WriteLine("Connecting...", "Debug");
            _client = new TcpClient(_host, _port);

            Debug.WriteLine("Getting stream...", "Debug");
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream);

            Debug.WriteLine("Creating thread...", "Debug");
            _listenThread = new Thread(Listen);

            Debug.WriteLine("Starting thread...", "Debug");
            _listenThread.Start();

            IRCMessage message;

            // Send USER
            message = new IRCMessage();
            message.Command = "USER";
            message.Params.Add(_nick);
            message.Params.Add("0");
            message.Params.Add("*");
            message.Message = _nick;
            SendMessage(message);

            message = new IRCMessage();
            message.Command = "NICK";
            message.Params.Add(_nick);
            SendMessage(message);
        }

        #region Channels

        public void Join(string channel)
        {
            if (String.IsNullOrEmpty(channel))
                throw new ArgumentNullException("channel");

            channel = channel.ToLowerInvariant();
            if (_channels.Contains(channel))
                return;

            IRCMessage message = new IRCMessage();
            message.Command = "JOIN";
            message.Params.Add("#" + channel);
            SendMessage(message);

            _channels.Add(channel);
        }

        public void Leave(string channel)
        {
            if (String.IsNullOrEmpty(channel))
                throw new ArgumentNullException("channel");

            channel = channel.ToLowerInvariant();
            if (!_channels.Contains(channel))
                return;

            IRCMessage message = new IRCMessage();
            message.Command = "PART";
            message.Params.Add("#" + channel);
            SendMessage(message);

            _channels.Remove(channel);
        }

        #endregion

        #region Methods - Private

        /// <summary>
        ///  Handles the PING message
        /// </summary>
        void HandlePing(IRCMessage message)
        {
            message.Command = "PONG";
            SendMessage(message);
        }

        private void Listen()
        {
            try
            {
                while (true)
                {
                    IRCMessage message = IRCMessage.FromString(_reader.ReadLine());

                    // Setup some basic handles
                    if (message.Command == "PING")
                        HandlePing(message);

                    RaiseMessageReceived(message);
                }
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("Stopped listening...");
            }
        }

        #endregion

        #region IIRCConnection Members

        public void SendMessage(IRCMessage message)
        {
            _writer.WriteLine(message.ToString());
            _writer.Flush();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // Kill the listener thread somehow,
            // when its stuck in the blocking readline call...
            _listenThread.Abort();
        }

        #endregion

    }
}
