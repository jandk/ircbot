
using System;
using System.Collections.Generic;
using System.IO;

namespace Parsing
{

	#region Extensions

	static class Extensions
	{

		public static bool IsDec(this int c)
		{
			return c >= '0' && c <= '9';
		}

		public static int FromDec(this int c)
		{
			if (c >= '0' && c <= '9')
				return c - '0';

			throw new Exception("Invalid decimal character");
		}


		public static bool IsHex(this int c)
		{
			return
				(c >= '0' && c <= '9') ||
				(c >= 'a' && c <= 'f') ||
				(c >= 'A' && c <= 'F');
		}

		public static int FromHex(this int c)
		{
			if (c >= '0' && c <= '9')
				return (c - '0');
			if (c >= 'a' && c <= 'f')
				return (c - 'a') + 10;
			if (c >= 'A' && c <= 'F')
				return (c - 'A') + 10;

			throw new Exception("Invalid hexadecimal character");
		}
		
		public static bool IsAlpha(this int c)
		{
			return
				(c >= 'a' && c <= 'z') ||
				(c >= 'A' && c <= 'Z');
		}
		
		public static char FromAlpha(this int c)
		{
			return (char)c;
		}
	}

	#endregion

	#region IToken

	interface IToken
	{
	}

	#endregion

	#region IScanner

	interface IScanner<T>
		where T : IToken
	{
		IEnumerator<T> Scan(TextReader reader);
	}

	#endregion

	#region Scanner

	abstract class ScannerBase<T>
		: IScanner<T>
		where T : IToken
	{

		protected TextReader _reader;

		public IEnumerator<T> Scan(TextReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			_reader = reader;
			_hasPeek = false;

			return Scan();
		}

		protected abstract IEnumerator<T> Scan();


		#region Helpers

		private bool _prevIsNl;
		private bool _hasPeek;
		private int _peek;
		private int _line;
		private int _column;

		protected int Peek()
		{
			if (!_hasPeek)
			{
				_peek = _reader.Read();
				_hasPeek = true;
			}

			return _peek;
		}

		protected int Read()
		{
			int c = _hasPeek ? _peek : _reader.Read();

			_hasPeek = false;

			if (_prevIsNl)
			{
				_line++;
				_column = 0;
				_prevIsNl = false;
			}

			// TODO: Support other newline characters
			if (c == '\n')
				_prevIsNl = true;

			_column++;

			return c;
		}

		protected bool Maybe(char c)
		{
			if (Peek() == c)
			{
				Read();
				return true;
			}

			return false;
		}

		protected void Expect(char e)
		{
			int c;
			if ((c = Read()) != e)
				Throw(String.Format("Expected '{0}', got '{1}'", e, c));
		}

		protected void Expect(string e)
		{
			for (int i = 0; i < e.Length; i++)
				if (Read() != e[i])
					Throw(String.Format("Error in expected string '{0}' on place '{1}'", e, i));
		}

		protected void Throw(string e)
		{
			throw new Exception(String.Format("Scanner error: {0} on Line {1}, Column {2}", e, _line, _column));
		}

		#endregion

	}

	#endregion

	#region IParser<T>

	interface IParser<T>
		where T : IToken
	{
		object Parse(IEnumerator<T> tokenStream);
	}

	#endregion

	#region Parser

	abstract class ParserBase<T>
		: IParser<T>
		where T : IToken
	{

		protected IEnumerator<T> _ts;

		public object Parse(IEnumerator<T> tokenStream)
		{
			if (tokenStream == null)
				throw new ArgumentNullException("tokenStream");

			_ts = tokenStream;

			return Parse();
		}

		protected abstract object Parse();

	}

	#endregion

}
