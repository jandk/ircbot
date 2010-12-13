
using System;
using System.Collections.Generic;

namespace IRCBot.Tools
{
	public class RingBuffer<T>
	{
		T[] _buffer;
		int _off;
		int _cap;
		int _count;

		public int Count
		{
			get { return _count; }
		}

		public bool IsFull
		{
			get { return _count == _cap; }
		}

		public T this[int index]
		{
			get
			{
				if (index < 0)
					throw new ArgumentOutOfRangeException("index");

				return _buffer[(_off + index) % _cap];
			}
			set
			{
				if (index < 0)
					throw new ArgumentOutOfRangeException("index");

				_buffer[(_off + index) % _cap] = value;
			}
		}

		public RingBuffer(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity");

			_cap = capacity;
			_buffer = new T[_cap];
		}

		public void Write(T item)
		{
			_buffer[_off] = item;

			if (++_off == _cap)
				_off = 0;

			if (_count < _cap)
				_count++;
		}

		public IEnumerable<T> IterateReverse()
		{
			int s = IsFull ? _off + _cap - 1 : _count - 1;
			int n = Math.Min(_count, _cap);

			for (int i = 0; i < n; i++)
			{
				int off = ((s - i) + _cap) % _cap;
				yield return _buffer[off];
			}
		}

		public IEnumerable<T> Iterate()
		{
			int s = IsFull ? _off : 0;
			int n = Math.Min(_count, _cap);

			for (int i = 0; i < n; i++)
			{
				int off = (s + i) % _cap;
				yield return _buffer[off];
			}
		}
	}
}
