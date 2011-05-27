
using System;
using System.IO;

using Calculator;
using IRC;

namespace IRCBot.Plugins
{
	class CalculatorPlugin
		: IRCPluginBase
	{

		static readonly Scanner Scanner = new Scanner();
		static readonly Parser Parser = new Parser();

		protected override bool Initialize()
		{
			Bot.SubscribeToMessage("^=", HandleCalculation);

			return true;
		}

		protected void HandleCalculation(IRCMessage message)
		{
			string formula = message.Message.Substring(1);
			double result = 0;

			try
			{
				using (var reader = new StringReader(formula))
					result = Parser.Parse(Scanner.Scan(reader));

				Bot.SendChannelMessage(
					message.Channel,
					String.Format("Result: {0}", result)
				);
			}
			catch (Exception ex)
			{
				Bot.SendChannelMessage(
					message.Channel,
					"Error: " + ex.Message
				);

				return;
			}
		}
	}
}

namespace Calculator
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	using TokenStream = System.Collections.Generic.IEnumerator<Token>;

	#region Extentions

	static class Extensions
	{
		public static bool IsDigit(this char c)
		{
			return c >= '0' && c <= '9';
		}

		public static bool IsWhiteSpace(this char c)
		{
			return c == ' ' || c == '\t' || c == '\r' || c == '\n';
		}

		public static char PeekChar(this TextReader reader)
		{
			return (char)reader.Peek();
		}

		public static char ReadChar(this TextReader reader)
		{
			return (char)reader.Read();
		}
	}

	#endregion

	#region Kind

	enum Kind
	{
		Eof,
		Plus,
		Minus,
		Mult,
		Div,
		LPar,
		RPar,
		Num
	}

	#endregion

	#region Token

	class Token
	{
		public Kind Kind { get; private set; }
		public double Value { get; private set; }

		private Token(Kind kind)
		{
			Kind = kind;
		}

		private Token(double value)
		{
			Kind = Kind.Num;
			Value = value;
		}

		public override string ToString()
		{
			return (Kind == Kind.Num)
				? String.Format("NUM({0})", Value)
				: Kind.ToString();
		}

		static public Token FromKind(Kind kind)
		{
			return new Token(kind);
		}

		static public Token FromDouble(double value)
		{
			return new Token(value);
		}
	}

	#endregion

	#region Scanner

	class Scanner
	{
		static double ScanReal(TextReader reader)
		{
			double n = 0;

			do
			{
				n = (10 * n) + (reader.Read() - '0');
			}
			while (reader.PeekChar().IsDigit());


			if (reader.Peek() == '.')
			{
				reader.Read();
				return ScanFrac(n, reader);
			}

			return n;
		}

		static double ScanFrac(double n, TextReader reader)
		{
			double w = 0.1;

			while (reader.PeekChar().IsDigit())
			{
				n += w * (reader.Read() - '0');
				w /= 10;
			}

			return n;
		}

		public IEnumerator<Token> Scan(TextReader reader)
		{
			while (reader.Peek() != -1)
			{
				if (reader.PeekChar().IsWhiteSpace())
				{
					reader.Read();
					continue;
				}

				if (reader.PeekChar().IsDigit())
				{
					yield return Token.FromDouble(ScanReal(reader));
					continue;
				}

				char c = reader.ReadChar();
				switch (c)
				{
					case '+':
						yield return Token.FromKind(Kind.Plus);
						break;
					case '-':
						yield return Token.FromKind(Kind.Minus);
						break;
					case '*':
						yield return Token.FromKind(Kind.Mult);
						break;
					case '/':
						yield return Token.FromKind(Kind.Div);
						break;
					case '(':
						yield return Token.FromKind(Kind.LPar);
						break;
					case ')':
						yield return Token.FromKind(Kind.RPar);
						break;
					default:
						throw new Exception("Illegal character: " + c);
				}
			}

			yield return Token.FromKind(Kind.Eof);
		}
	}

	#endregion

	#region Parser

	class Parser
	{
		public double Parse(TokenStream ts)
		{
			ts.MoveNext();
			double result = expr(ts);

			if (ts.Current.Kind != Kind.Eof)
				throw new Exception("Expected EOF");

			return result;
		}

		double expr(TokenStream ts)
		{
			return exprCont(term(ts), ts);
		}

		double exprCont(double inval, TokenStream ts)
		{
			switch (ts.Current.Kind)
			{
				case Kind.Plus:
					ts.MoveNext();
					return exprCont(inval + term(ts), ts);
				case Kind.Minus:
					ts.MoveNext();
					return exprCont(inval - term(ts), ts);
				default:
					return inval;
			}
		}

		double term(TokenStream ts)
		{
			return termCont(factor(ts), ts);
		}

		double termCont(double inval, TokenStream ts)
		{
			switch (ts.Current.Kind)
			{
				case Kind.Mult:
					ts.MoveNext();
					return termCont(inval * factor(ts), ts);
				case Kind.Div:
					ts.MoveNext();
					return termCont(inval / factor(ts), ts);
				default:
					return inval;
			}
		}

		double factor(TokenStream ts)
		{
			switch (ts.Current.Kind)
			{
				case Kind.Minus:
					ts.MoveNext();
					return -factor(ts);

				case Kind.Num:
					double value = ts.Current.Value;
					ts.MoveNext();
					return value;

				case Kind.LPar:
					ts.MoveNext();
					double exprValue = expr(ts);
					if (ts.Current.Kind != Kind.RPar)
						throw new Exception("Expected ')'");
					ts.MoveNext();
					return exprValue;

				default:
					throw new ApplicationException("Expected number or '('");
			}
		}
	}

	#endregion

}
