using System.Collections.Generic;
using Kingmaker.Controllers.Units.CameraFollow;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers.Units;

internal class TasksQueue<T> : List<T> where T : ICameraFollowTask
{
	public void Enqueue(T item)
	{
		int num = FindLastIndex((T x) => x.Priority < item.Priority);
		if (num != -1)
		{
			Insert(num + 1, item);
		}
		else
		{
			Insert(0, item);
		}
	}

	public void EnqueueFirst(T item)
	{
		int num = FindIndex((T x) => x.Priority > item.Priority);
		if (num != -1)
		{
			Insert(num, item);
		}
		else
		{
			Add(item);
		}
	}

	public T Dequeue()
	{
		T result = this.LastItem();
		this.RemoveLast();
		return result;
	}
}
