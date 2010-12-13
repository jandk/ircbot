
using System;
using System.IO;
using System.Collections.Generic;

namespace IRCBot.Tools
{
	static class Extensions
	{
		public static IEnumerable<string> ReadLines(this TextReader reader)
		{
			string line;

			while ((line = reader.ReadLine()) != null)
				yield return line;
		}
	}
}
