using System.Collections.Generic;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.UI.Legacy.Ping;

public class PingManager : MonoBehaviour
{
	protected static PingManager s_Instance;

	protected List<PingListener> m_PingHandlers = new List<PingListener>();

	public List<string> m_PingNow = new List<string>();

	public static PingManager Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new GameObject("[PingManager]").AddComponent<PingManager>();
			}
			return s_Instance;
		}
	}

	public List<PingListener> PingHandlers => m_PingHandlers = ((m_PingHandlers == null) ? new List<PingListener>() : m_PingHandlers);

	public void Register(PingListener handler)
	{
		if (!m_PingHandlers.Contains(handler))
		{
			m_PingHandlers.Add(handler);
		}
	}

	public void Unregister(PingListener handler)
	{
		m_PingHandlers.Remove(handler);
	}

	public bool IsPing(string code)
	{
		return m_PingNow.Contains(code);
	}

	public void CancelPing()
	{
		if (m_PingHandlers == null)
		{
			return;
		}
		for (int i = 0; i < m_PingHandlers.Count; i++)
		{
			PingListener pingListener = m_PingHandlers[i];
			if (!(pingListener == null))
			{
				pingListener.Ping(active: false, awaitFalse: false);
			}
		}
	}

	public void Ping(string code, bool active)
	{
		if (string.IsNullOrEmpty(code))
		{
			return;
		}
		bool flag = IsPing(code);
		if (active ? flag : (!flag))
		{
			return;
		}
		if (flag)
		{
			m_PingNow.Remove(code);
		}
		else
		{
			m_PingNow.Add(code);
		}
		foreach (PingListener item in m_PingHandlers.FindAll((PingListener x) => x.Code == code))
		{
			item.Ping(active, awaitFalse: true);
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!ApplicationFocusEvents.PingDisabled)
		{
			CancelPing();
		}
	}
}
