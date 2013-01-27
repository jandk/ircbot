
using System.Collections.Generic;
using System.IO;

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
