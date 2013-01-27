
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Parsing.Json
{

	#region Extensions

	static class Extensions
	{
		public static bool IsWhiteSpace(this int c)
		{
			switch (c)
			{
				case '\t':
				case '\n':
				case '\r':
				case ' ':
					return true;
				default:
					return false;
			}
		}
	}

	#endregion

	#region Kind

	enum Kind
	{
		Eof,
		Colon,
		Comma,
		BraceLeft,
		BraceRight,
		BracketLeft,
		BracketRight,

		// Values
		Number,
		String,
		Boolean,
		Null
	}

	#endregion

	#region Token

	class Token
		: IToken
	{
		private readonly Kind _kind;
		private readonly object _value;


		public Kind Kind { get { return _kind; } }
		public object Value { get { return _value; } }


		private Token(Kind kind)
		{
			_kind = kind;
		}

		private Token(Kind kind, object value)
		{
			_kind = kind;
			_value = value;
		}


		public bool IsValue
		{
			get
			{
				switch (_kind)
				{
					case Kind.String:
					case Kind.Number:
					case Kind.Boolean:
					case Kind.Null:
						return true;
					default:
						return false;
				}
			}
		}


		public override string ToString()
		{
			return String.Format(
				"[Kind: {0}, Value: {1}]",
				Kind.ToString(),
				Value ?? String.Empty
			);
		}

		static public Token FromKind(Kind kind)
		{
			return new Token(kind);
		}

		static public Token FromString(string value)
		{
			return new Token(Kind.String, value);
		}

		static public Token FromNumber(double value)
		{
			return new Token(Kind.Number, value);
		}

		static public Token FromBoolean(bool value)
		{
			return new Token(Kind.Boolean, value);
		}

		static public Token FromNull()
		{
			return new Token(Kind.Null, null);
		}
	}

	#endregion

	#region Scanner

	class Scanner
		: ScannerBase<Token>
	{

		private static readonly StringBuilder _sb = new StringBuilder(1024);

		#region Strings

		string ScanString()
		{
			_sb.Length = 0;

			Read();

			while (true)
			{
				int c = Read();

				if (c == '"')
					break;

				if (c == '\\')
				{
					switch (Read())
					{
						case '\\': c = '\\'; break;
						case '"': c = '"'; break;
						case '/': c = '/'; break;
						case 'b': c = '\b'; break;
						case 'f': c = '\f'; break;
						case 'n': c = '\n'; break;
						case 'r': c = '\r'; break;
						case 't': c = '\t'; break;
						case 'u':
							c = (char)(
								Read().FromHex() << 12 |
								Read().FromHex() << 8 |
								Read().FromHex() << 4 |
								Read().FromHex()
							);
							break;
						default:
							Throw("Invalid escape sequence");
							break;
					}
				}

				_sb.Append((char)c);
			}

			return _sb.ToString();
		}

		#endregion

		#region Numbers

		double ScanNumber()
		{
			bool negate = Maybe('-');

			if (!Peek().IsDec())
				Throw("Expected a digit");

			// Integer part
			if (Maybe('0'))
				return 0;

			double d = 0;
			while (Peek().IsDec())
				d = (d * 10) + Read().FromDec();

			// Fractional part
			if (Maybe('.'))
			{
				double f = 0;
				double w = 0.1;

				if (!Peek().IsDec())
					Throw("At least one digit after '.'");

				while (Peek().IsDec())
				{
					f += w * Read().FromDec();
					w *= 0.1;
				}

				d += f;
			}

			// Exponent
			if (Maybe('e') || Maybe('E'))
			{
				bool negateExponent = false;

				if (Peek() == '+' || Peek() == '-')
				{
					negateExponent = Peek() == '-';
					Read();
				}

				if (!Peek().IsDec())
					Throw("At least one digit after 'e' or 'E'.");

				double e = 0;
				while (Peek().IsDec())
					e = (e * 10) + Read().FromDec();

				if (negateExponent)
					e = -e;

				d *= Math.Pow(10, e);
			}

			if (negate)
				d = -d;

			return d;
		}

		#endregion

		#region IScanner<Token> Members

		protected override IEnumerator<Token> Scan()
		{
			while (Peek() != -1)
			{
				int peek = Peek();

				if (peek.IsWhiteSpace())
				{
					Read();
					continue;
				}

				if (peek.IsDec() || peek == '-')
				{
					double value = ScanNumber();
					yield return Token.FromNumber(value);
					continue;
				}

				switch (peek)
				{
					case '"':
						string value = ScanString();
						yield return Token.FromString(value);
						break;

					case 't':
						Expect("true");
						yield return Token.FromBoolean(true);
						break;

					case 'f':
						Expect("false");
						yield return Token.FromBoolean(false);
						break;

					case 'n':
						Expect("null");
						yield return Token.FromNull();
						break;

					// Operators come last
					case '{': Read();
						yield return Token.FromKind(Kind.BraceLeft);
						break;

					case '}': Read();
						yield return Token.FromKind(Kind.BraceRight);
						break;

					case '[': Read();
						yield return Token.FromKind(Kind.BracketLeft);
						break;

					case ']': Read();
						yield return Token.FromKind(Kind.BracketRight);
						break;

					case ':': Read();
						yield return Token.FromKind(Kind.Colon);
						break;

					case ',': Read();
						yield return Token.FromKind(Kind.Comma);
						break;

					default:
						Throw("Invalid character: " + Peek());
						break;
				}

			}

			yield return Token.FromKind(Kind.Eof);
		}

		#endregion

	}


	#endregion

	#region Parser

	class Parser
		: ParserBase<Token>
	{

		protected override object Parse()
		{
			_ts.MoveNext();

			object value = JsonValue();

			_ts.MoveNext();
			if (_ts.Current.Kind != Kind.Eof)
				throw new Exception("Expected EOF");

			return value;
		}

		ArrayList JsonArray()
		{
			var list = new ArrayList();

			_ts.MoveNext();
			if (_ts.Current.Kind == Kind.BracketRight)
				return list;

			while (true)
			{
				list.Add(JsonValue());

				_ts.MoveNext();
				switch (_ts.Current.Kind)
				{
					case Kind.Comma:
						_ts.MoveNext();
						continue;
					case Kind.BracketRight:
						return list;
					default:
						throw new Exception("Expected 'Colon' or 'BracketRight'.");
				}
			}
		}

		Hashtable JsonObject()
		{
			var dict = new Hashtable();

			_ts.MoveNext();
			if (_ts.Current.Kind == Kind.BraceRight)
				return dict;

			while (true)
			{
				if (_ts.Current.Kind != Kind.String)
					throw new Exception("Expected 'String' or 'BraceRight', got " + _ts.Current.Kind.ToString());

				var name = _ts.Current.Value.ToString();

				_ts.MoveNext();
				if (_ts.Current.Kind != Kind.Colon)
					throw new Exception("Expected 'Colon', got " + _ts.Current.Kind.ToString());

				_ts.MoveNext();
				dict.Add(name, JsonValue());

				_ts.MoveNext();
				switch (_ts.Current.Kind)
				{
					case Kind.Comma:
						_ts.MoveNext();
						continue;
					case Kind.BraceRight:
						return dict;
					default:
						throw new Exception("Expected a comma or a right brace");
				}
			}
		}

		object JsonValue()
		{
			if (_ts.Current.IsValue)
				return _ts.Current.Value;

			switch (_ts.Current.Kind)
			{
				case Kind.BracketLeft:
					return JsonArray();
				case Kind.BraceLeft:
					return JsonObject();
				default:
					throw new Exception("Expected 'BracketLeft' or 'BraceLeft', got " + _ts.Current.Kind.ToString());
			}
		}
	}

	#endregion

	#region JsonWriter

	class JsonWriter
	{
		readonly StringBuilder _sb = new StringBuilder(1024);

		public static string Write(object value)
		{
			var writer = new JsonWriter();
			writer.Convert(value);
			return writer._sb.ToString();
		}


		void Convert(object value)
		{
			var arrayList = value as ArrayList;
			if (arrayList != null)
			{
				_sb.Append('[');

				foreach (var item in arrayList)
				{
					Convert(item);
					_sb.Append(',');
				}

				if (arrayList.Count > 0)
					_sb.Length -= 1;

				_sb.Append(']');
				return;
			}

			var hashtable = value as Hashtable;
			if (hashtable != null)
			{
				_sb.Append('{');

				foreach (var item in hashtable)
				{
					var entry = (DictionaryEntry)item;

					Convert(entry.Key.ToString());
					_sb.Append(':');
					Convert(entry.Value);
					_sb.Append(',');
				}

				if (hashtable.Count > 0)
					_sb.Length -= 1;

				_sb.Append('}');
				return;
			}

			var s = value as string;
			if (s != null)
			{
				_sb.Append('"');
				foreach (char c in s)
				{
					switch (c)
					{
						case '"': _sb.Append("\\\""); break;
						case '\\': _sb.Append("\\\\"); break;
						case '/': _sb.Append("\\/"); break;
						case '\b': _sb.Append("\\b"); break;
						case '\f': _sb.Append("\\f"); break;
						case '\n': _sb.Append("\\n"); break;
						case '\r': _sb.Append("\\r"); break;
						case '\t': _sb.Append("\\t"); break;
						default: _sb.Append(c); break;
					}
				}
				_sb.Append('"');
				return;
			}

			if (value is double || value is int)
				_sb.Append(((IFormattable)value).ToString("G", NumberFormatInfo.InvariantInfo));

			else if (value is bool)
				_sb.Append((bool)value ? "true" : "false");

			else if (value == null)
				_sb.Append("null");

			else throw new Exception("Cannot convert type");
		}
	}

	#endregion

}
