using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mono.Collections.Generic;

[ComVisible(false)]
public sealed class ReadOnlyCollection<T> : Collection<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection
{
	private static ReadOnlyCollection<T> empty;

	public static ReadOnlyCollection<T> Empty
	{
		get
		{
			if (empty != null)
			{
				return empty;
			}
			Interlocked.CompareExchange(ref empty, new ReadOnlyCollection<T>(), null);
			return empty;
		}
	}

	bool ICollection<T>.IsReadOnly => true;

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	private ReadOnlyCollection()
	{
	}

	public ReadOnlyCollection(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException();
		}
		Initialize(array, array.Length);
	}

	public ReadOnlyCollection(Collection<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException();
		}
		Initialize(collection.items, collection.size);
	}

	private void Initialize(T[] items, int size)
	{
		base.items = new T[size];
		Array.Copy(items, 0, base.items, 0, size);
		base.size = size;
	}

	internal override void Grow(int desired)
	{
		throw new InvalidOperationException();
	}

	protected override void OnAdd(T item, int index)
	{
		throw new InvalidOperationException();
	}

	protected override void OnClear()
	{
		throw new InvalidOperationException();
	}

	protected override void OnInsert(T item, int index)
	{
		throw new InvalidOperationException();
	}

	protected override void OnRemove(T item, int index)
	{
		throw new InvalidOperationException();
	}

	protected override void OnSet(T item, int index)
	{
		throw new InvalidOperationException();
	}
}
