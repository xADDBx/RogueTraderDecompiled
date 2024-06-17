using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Utility.ManualCoroutines;

public readonly struct CoroutineHandler
{
	public static readonly CoroutineHandler Empty;

	private readonly CoroutineManager m_Manager;

	private readonly IEnumerator m_Enumerator;

	private readonly bool m_UseObjectHolder;

	private readonly Object m_ObjectHolder;

	public bool IsRunning
	{
		get
		{
			if (m_Enumerator != null && (!m_UseObjectHolder || m_ObjectHolder != null))
			{
				return m_Manager.IsRunning(m_Enumerator);
			}
			return false;
		}
	}

	public CoroutineHandler([NotNull] CoroutineManager manager, [NotNull] IEnumerator enumerator, [CanBeNull] Object objectHolder = null)
	{
		m_Manager = manager;
		m_Enumerator = enumerator;
		m_UseObjectHolder = objectHolder != null;
		m_ObjectHolder = objectHolder;
	}

	public void Stop()
	{
		m_Manager?.Stop(m_Enumerator);
	}
}
