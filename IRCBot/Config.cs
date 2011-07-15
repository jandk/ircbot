
using System;
using System.IO;
using System.Collections.Specialized;

namespace IRCBot
{
	class Config
		: Singleton<Config>
	{

		private StringDictionary _config;

		private Config() { }

		public string this[string key]
		{
			get
			{
				if (String.IsNullOrEmpty(key))
					throw new ArgumentNullException("Invalid empty key");

				if (!_config.ContainsKey(key))
					throw new Exception("Key does not exist");

				return _config[key];
			}
		}

		public void Initialize(string filename)
		{
			if (String.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			ReadConfig(filename);
		}

		private void ReadConfig(string filename)
		{
			if (!File.Exists(filename))
				throw new Exception("ConfigFile does not exist");

			_config = new StringDictionary();
			string[] lines = File.ReadAllLines(filename);
			foreach (var line in lines)
			{
				if (line.IndexOf('=') <= 0)
					continue;

				var split = line.Split(new char[] { '=' }, 2);

				if (!_config.ContainsKey(split[0].Trim()))
					_config.Add(split[0].Trim(), split[1].Trim());
			}
		}

	}
}
