
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

using IRC;
using Parsing.Json;

namespace IRCBot.Plugins
{
	class UrlShortenerPlugin
		: IRCPluginBase
	{
		const string UrlRegexString = @"\b(https?|ftp)://[-A-Z0-9+&@#/%?=~_|$!:,.;]*[A-Z0-9+&@#/%=~_|$]";
		static readonly Regex UrlRegex = new Regex(UrlRegexString, RegexOptions.IgnoreCase);

		static readonly string ApiKey = Config.Instance["url.key"];
		static readonly Uri ApiUrl = new Uri(@"https://www.googleapis.com/urlshortener/v1/url?key=" + ApiKey);

		static readonly string[] UrlShorteners = new[]
        {
            "goo.gl",
            "youtu.be"
        };


		private readonly WebClient _poster = new WebClient();
		private readonly Scanner _scanner = new Scanner();
		private readonly Parser _parser = new Parser();

		protected override void Initialize()
		{
			Bot.SubscribeToMessage(UrlRegexString, ShortenUrl);

			_poster.UploadStringCompleted += HandleReply;
		}

		protected void ShortenUrl(IRCMessage message)
		{
			var match = UrlRegex.Match(message.Message);
			var uri = new Uri(message.Message.Substring(match.Index, match.Length));

			// Skip shortened urls
			if (Array.IndexOf(UrlShorteners, uri.Host) >= 0)
				return;

			// Skip short urls
			if (match.Value.Length <= 20)
				return;

			// Create new json
			var table = new Hashtable { { "longUrl", match.Value } };
			string json = JsonWriter.Write(table);

			// Post the data to the google url api
			_poster.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			_poster.UploadStringAsync(ApiUrl, null, json, message);
		}

		void HandleReply(object sender, UploadStringCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				Console.WriteLine("Got status: " + e.Error);
				return;
			}

			// Google api always returns an object
			var result = (Hashtable)_parser.Parse(_scanner.Scan(new StringReader(e.Result)));

			string message;
			if (result.ContainsKey("error"))
				message = "Error while shortening url.";
			else if (result.ContainsKey("id"))
				message = "Short url: " + result["id"];
			else
				message = "Unknown error";

			string channel = ((IRCMessage)e.UserState).Channel;
			Bot.SendChannelMessage(channel, message);
		}
	}
}
