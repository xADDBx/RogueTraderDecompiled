using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class RegistrationService<T> where T : RegistrationService<T>.DatedItem
{
	public class DatedItem
	{
		public DateTime Time { get; set; }
	}

	private readonly int _maxItems;

	private readonly ConcurrentQueue<T> _registered = new ConcurrentQueue<T>();

	public int Length => _registered.Count;

	public RegistrationService(int maxItems)
	{
		_maxItems = maxItems;
	}

	public void Register(T item)
	{
		_registered.Enqueue(item);
		if (_registered.Count > _maxItems)
		{
			_registered.TryDequeue(out var _);
		}
	}

	public IEnumerable<T> GetLastFrom(DateTime dateTime)
	{
		return from i in _registered.Reverse()
			where i.Time > dateTime
			select i;
	}

	public IEnumerable<T> GetInInterval(DateTime from, DateTime to)
	{
		return (from i in _registered.Reverse()
			where i.Time >= @from && i.Time <= to
			select i).ToList();
	}

	public T? Get(int index)
	{
		if (index > 0)
		{
			throw new ArgumentOutOfRangeException("index", "Should be lesser than zero");
		}
		if (-index > Length - 1)
		{
			return null;
		}
		return _registered.Reverse().Skip(-index).FirstOrDefault();
	}

	public void Clear()
	{
		_registered.Clear();
	}
}
