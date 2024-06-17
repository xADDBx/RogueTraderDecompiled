using System.Collections.Generic;
using Pathfinding.Util;

namespace Kingmaker.PubSubSystem.Core;

public class SubscribersList<TSubscriber> : IAstarPooledObject where TSubscriber : class
{
	private bool m_NeedsCleanUp;

	public bool Executing;

	protected internal readonly List<object> List = new List<object>();

	public virtual void AddSubscriber(object subscriber)
	{
		List.Add(subscriber);
	}

	public virtual void RemoveSubscriber(object subscriber)
	{
		if (Executing)
		{
			int num = List.IndexOf(subscriber);
			if (num >= 0)
			{
				m_NeedsCleanUp = true;
				List[num] = null;
			}
		}
		else
		{
			List.Remove(subscriber);
		}
	}

	public void Cleanup()
	{
		if (m_NeedsCleanUp)
		{
			List.RemoveAll((object s) => s == null);
			m_NeedsCleanUp = false;
		}
	}

	public void OnEnterPool()
	{
		Executing = false;
		List.Clear();
	}
}
