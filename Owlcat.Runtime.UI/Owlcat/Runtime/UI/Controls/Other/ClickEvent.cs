using System;
using UnityEngine.Events;

namespace Owlcat.Runtime.UI.Controls.Other;

[Serializable]
public class ClickEvent : UnityEvent
{
	private int m_ListenersCount;

	private int m_PersistentListenersCount = -1;

	public int AllListenersCount
	{
		get
		{
			if (m_PersistentListenersCount < 0)
			{
				m_PersistentListenersCount = GetPersistentEventCount();
			}
			return m_ListenersCount + m_PersistentListenersCount;
		}
	}

	public new void AddListener(UnityAction call)
	{
		m_ListenersCount++;
		base.AddListener(call);
	}

	public new void RemoveListener(UnityAction call)
	{
		m_ListenersCount--;
		base.RemoveListener(call);
	}
}
