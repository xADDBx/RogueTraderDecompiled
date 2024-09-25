using System.Collections.Generic;

namespace Kingmaker.Controllers.Units;

internal class TasksQueue<T> : LinkedList<T>
{
	public void Enqueue(T item)
	{
		AddLast(item);
	}

	public T Dequeue()
	{
		T value = base.First.Value;
		RemoveFirst();
		return value;
	}
}
