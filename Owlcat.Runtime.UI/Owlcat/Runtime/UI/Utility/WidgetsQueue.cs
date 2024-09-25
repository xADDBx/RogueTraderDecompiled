using System.Collections.Generic;

namespace Owlcat.Runtime.UI.Utility;

public class WidgetsQueue<T> : LinkedList<T>
{
	public void Enqueue(T item)
	{
		AddFirst(item);
	}

	public T Dequeue()
	{
		T value = base.First.Value;
		RemoveFirst();
		return value;
	}
}
