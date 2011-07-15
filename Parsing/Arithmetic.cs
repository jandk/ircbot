

using System;
using System.Collections.Generic;

namespace Parsing.Arithmetic
{

	#region Kind

	enum Kind
	{
		Eof,
		ParenLeft,
		ParenRight,
		Number,

		// Operators
		OpAdd,
		OpSubtract,
		OpMultiply,
		OpDivide,
	}

	#endregion

	#region Token

	class Token
		: IToken
	{

		private Kind _kind;
		private double _value;


		public Kind Kind { get { return _kind; } }
		public double Value { get { return _value; } }


		private Token(Kind kind)
		{
			_kind = kind;
		}

		private Token(double value)
		{
			_kind = Kind.Number;
			_value = value;
		}


		public override string ToString()
		{
			switch (_kind)
			{
				case Kind.Number:
					return "Number: " + _value.ToString();
				default:
					return _kind.ToString();
			}
		}


		static public Token FromKind(Kind kind)
		{
			return new Token(kind);
		}

		static public Token FromNumber(double value)
		{
			return new Token(value);
		}
	}

	#endregion

	#region Scanner

	class Scanner
		: ScannerBase<Token>
	{

		#region Scanner<Token> Members

		protected override IEnumerator<Token> Scan()
		{
			while (Peek() != -1)
			{
				if (Maybe(' '))
					continue;

				if (Peek().IsDec())
				{
					double value = ScanNumber();
					yield return Token.FromNumber(value);
					continue;
				}

				#region Operators

				int read;
				switch (read = Read())
				{
					case '+':
						yield return Token.FromKind(Kind.OpAdd);
						break;

					case '-':
						yield return Token.FromKind(Kind.OpSubtract);
						break;

					case '*':
						yield return Token.FromKind(Kind.OpMultiply);
						break;

					case '/':
						yield return Token.FromKind(Kind.OpDivide);
						break;

					case '(':
						yield return Token.FromKind(Kind.ParenLeft);
						break;

					case ')':
						yield return Token.FromKind(Kind.ParenRight);
						break;

					default:
						Throw(String.Format("Illegal charcter '{0}'", read));
						break;
				}

				#endregion

			}

			yield return Token.FromKind(Kind.Eof);
		}

		#endregion

		#region Numbers

		double ScanNumber()
		{
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
				bool negate = false;

				if (Peek() == '+' || Peek() == '-')
				{
					negate = Peek() == '-';
					Read();
				}

				if (!Peek().IsDec())
					Throw("At least one digit after 'e' or 'E'.");

				double e = 0;
				while (Peek().IsDec())
					e = (e * 10) + Read().FromDec();

				if (negate)
					e = -e;

				d *= Math.Pow(10, e);
			}

			return d;
		}

		#endregion

	}

	#endregion

	#region Parser

	class Parser
		: ParserBase<Token>
	{

		#region IParser<T> Members

		protected override object Parse()
		{
			_ts.MoveNext();

			double value = expr();

			if (_ts.Current.Kind != Kind.Eof)
				throw new Exception("Expected EOF");

			return value;
		}

		#endregion

		double expr()
		{
			return exprCont(term());
		}

		double exprCont(double inval)
		{
			switch (_ts.Current.Kind)
			{
				case Kind.OpAdd:
					_ts.MoveNext();
					return exprCont(inval + term());
				case Kind.OpSubtract:
					_ts.MoveNext();
					return exprCont(inval - term());
				default:
					return inval;
			}
		}

		double term()
		{
			return termCont(factor());
		}

		double termCont(double inval)
		{
			switch (_ts.Current.Kind)
			{
				case Kind.OpMultiply:
					_ts.MoveNext();
					return termCont(inval * factor());
				case Kind.OpDivide:
					_ts.MoveNext();
					return termCont(inval / factor());
				default:
					return inval;
			}
		}

		double factor()
		{
			switch (_ts.Current.Kind)
			{
				case Kind.Number:
					double value = _ts.Current.Value;
					_ts.MoveNext();
					return value;

				case Kind.ParenLeft:
					_ts.MoveNext();
					double exprValue = expr();
					if (_ts.Current.Kind != Kind.ParenRight)
						throw new Exception("Parse error: expected ')'");
					_ts.MoveNext();
					return exprValue;

				default:
					throw new ApplicationException("Parse error: expected number or '('");
			}
		}
	}

	#endregion

}
