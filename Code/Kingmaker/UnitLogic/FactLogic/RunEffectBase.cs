using System;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

public abstract class RunEffectBase
{
	[HideInInspector]
	public bool WasActivated;

	private Action m_CompleteCallback;

	public virtual void Activate(Action completeCallback)
	{
		m_CompleteCallback = completeCallback;
	}

	public abstract void Deactivate();

	protected void OnActivate()
	{
		WasActivated = true;
		m_CompleteCallback?.Invoke();
		m_CompleteCallback = null;
	}

	protected virtual void OnDeactivate()
	{
	}
}
