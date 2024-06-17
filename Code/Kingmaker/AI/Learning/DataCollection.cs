using System;
using System.Collections;
using System.Collections.Generic;

namespace Kingmaker.AI.Learning;

public abstract class DataCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	public virtual int Count
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public virtual bool IsReadOnly => false;

	public abstract void Add(T item);

	public abstract void Clear();

	public abstract bool Contains(T item);

	public abstract void CopyTo(T[] array, int arrayIndex);

	public abstract bool Remove(T item);

	public abstract IEnumerator<T> GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
