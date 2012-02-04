

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Parsing.Arithmetic
{

	#region Kind

	enum Kind
	{
		Eof,
		ParenLeft,
		ParenRight,
		Number,
		Identifier,

		// Operators
		OpAdd,
		OpSubtract,
		OpMultiply,
		OpDivide,
		OpModulo,
		OpPower,
		OpFactorial,
	}

	#endregion

	#region Token

	class Token
		: IToken
	{

		private Kind _kind;
		private double _dblValue;
		private string _strValue;


		public Kind Kind { get { return _kind; } }
		public double DoubleValue { get { return _dblValue; } }
		public string StringValue { get { return _strValue; } }


		private Token(Kind kind)
		{
			_kind = kind;
		}

		private Token(double value)
		{
			_kind = Kind.Number;
			_dblValue = value;
		}
		
		private Token(string value)
		{
			_kind = Kind.Identifier;
			_strValue = value;
		}


		public override string ToString()
		{
			switch (_kind)
			{
				case Kind.Number:
					return "Number: " + _dblValue.ToString(CultureInfo.InvariantCulture);
				case Kind.Identifier:
					return "Identifier: " + _strValue;
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
		
		static public Token FromIdentifier(string value)
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
				
				if (Peek().IsAlpha())
				{
					string value = ScanIdentifier();
					yield return Token.FromIdentifier(value);
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
					
					case '%':
						yield return Token.FromKind(Kind.OpModulo);
						break;
					
					case '!':
						yield return Token.FromKind(Kind.OpFactorial);
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
			double d = 0;
			if (!Maybe('0'))
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
		
		#region Identifiers
		
		string ScanIdentifier()
		{
			if(!Peek().IsAlpha())
				Throw("Expected a letter");
			
			string identifier = String.Empty;
			while(Peek().IsAlpha())
				identifier += Read().FromAlpha();
			
			return identifier;
		}
		
		#endregion

	}

	#endregion

	#region Parser

	class Parser
		: ParserBase<Token>
	{
		
		#region Defines
		
		static readonly Dictionary<string, Func<double,double>> Functions
			= new Dictionary<string, Func<double, double>>()
		{
			{"log2", Log2},
			{"log10", Math.Log10},
			{"log", Math.Log},
			{"exp", Math.Exp},
			{"sqrt", Math.Sqrt},
			{"floor", Math.Floor},
			{"ceil", Math.Ceiling},
			{"sin", Math.Sin},
			{"cos", Math.Cos},
			{"tan", Math.Tan},
			{"asin", Math.Asin},
			{"acos", Math.Acos},
			{"atan", Math.Atan},
			{"sinh", Math.Sinh},
			{"cosh", Math.Cosh},
			{"tanh", Math.Tanh},
		};
		
		static readonly Dictionary<string, double> Constants
			= new Dictionary<string, double>()
		{
			{"e", Math.E},
			{"pi", Math.PI},
		};
		
		static double Log2(double value)
		{
			return Math.Log(value) / Math.Log(2);
		}
		
		#endregion

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
				case Kind.OpModulo:
					_ts.MoveNext();
					return termCont(inval % factor());
				default:
					return inval;
			}
		}

		double factor()
		{
			switch (_ts.Current.Kind)
			{
				case Kind.Identifier:
					if (Constants.ContainsKey(_ts.Current.StringValue))
					{
						string identifier = _ts.Current.StringValue;
						_ts.MoveNext();
						return Constants[identifier];
					}
				
					if (Functions.ContainsKey(_ts.Current.StringValue))
					{
						string identifier = _ts.Current.StringValue;
						_ts.MoveNext();
						if (_ts.Current.Kind != Kind.ParenLeft)
							throw new Exception("Parse error: expected '('");
						_ts.MoveNext();
						double exprValue = expr();
						if (_ts.Current.Kind != Kind.ParenRight)
							throw new Exception("Parse error: expected ')'");
						_ts.MoveNext();
					
						return Functions[identifier](exprValue);
					}
				
					throw new Exception("Parse error: invalid identifier - " + _ts.Current.StringValue);

				case Kind.Number:
					double value = _ts.Current.DoubleValue;
					_ts.MoveNext();
					if (_ts.Current.Kind == Kind.OpFactorial)
					{
						value = factorial(value);
						_ts.MoveNext();
					}
					return value;

				case Kind.ParenLeft:
					_ts.MoveNext();
					double exprValue = expr();
					if (_ts.Current.Kind != Kind.ParenRight)
						throw new Exception("Parse error: expected ')'");
					_ts.MoveNext();
					return exprValue;

				default:
					throw new Exception("Parse error: expected number or '('");
			}
		}
		
		static double factorial(double value)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException("value", "cannot be negative");
			
			if (value == 0)
				return 1;
			
			double fact = 1;
			for(int i = 2; i <= (int)value; i++)
				fact *= i;
			
			return fact;
		}
	}

	#endregion

}
