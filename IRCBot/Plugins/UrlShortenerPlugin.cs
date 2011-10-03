
using System;
using System.Collections;
using System.Text.RegularExpressions;

using IRC;
using System.Net;
using System.Text;
using Parsing.Json;
using System.IO;

namespace IRCBot.Plugins
{
	class UrlShortenerPlugin
		: IRCPluginBase
	{
		const string UrlRegex = @"\b(https?|ftp)://[-A-Za-z0-9+&@#/%?=~_|$!:,.;]*[A-Za-z0-9+&@#/%=~_|$]";
		static readonly string ApiKey = Config.Instance["url.key"];
		static readonly Uri ApiUrl = new Uri(@"https://www.googleapis.com/urlshortener/v1/url?key=" + ApiKey);

		protected WebClient poster = new WebClient();
		protected Scanner scanner = new Scanner();
		protected Parser parser = new Parser();

		protected override bool Initialize()
		{
			Bot.SubscribeToMessage(UrlRegex, ShortenUrl);

			poster.UploadStringCompleted += HandleReply;

			return true;
		}

		protected void ShortenUrl(IRCMessage message)
		{
			if (message.Message.StartsWith("!"))
				return;

			Match match = Regex.Match(message.Message, UrlRegex);

			// Skip shortened urls
			if (match.Value.Contains("goo.gl"))
				return;

			// Skip short urls
			if (match.Value.Length <= 20)
				return;

			// Create new json
			Hashtable table = new Hashtable();
			table.Add("longUrl", match.Value);
			string json = JsonWriter.Write(table);

			// Post the data to the google url api
			poster.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			poster.UploadStringAsync(ApiUrl, null, json, message);
		}

		void HandleReply(object sender, UploadStringCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				Console.WriteLine("Got status: " + e.Error.ToString());
				return;
			}

			// Google api always returns an object
			Hashtable result = (Hashtable)parser.Parse(scanner.Scan(new StringReader(e.Result)));

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
