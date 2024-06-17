using System.Collections;
using UnityEngine;

namespace Kingmaker.UI.Legacy.Ping;

public class PingListener : MonoBehaviour
{
	public string Code;

	protected IPingAction[] m_ActionComponents;

	protected bool AbortCooldown;

	public bool IsActivePing { get; protected set; }

	protected void OnEnable()
	{
		m_ActionComponents = GetComponentsInChildren<IPingAction>(includeInactive: true);
		PingManager.Instance.Register(this);
		if (IsActivePing)
		{
			InternalPing(active: true);
		}
	}

	protected void OnDisable()
	{
		PingManager.Instance.Unregister(this);
		Ping(active: false, awaitFalse: false);
	}

	public void Ping(bool active, bool awaitFalse)
	{
		if (active)
		{
			if (!IsActivePing)
			{
				InternalPing(active: true);
				IsActivePing = true;
			}
			AbortCooldown = true;
		}
		else if (awaitFalse)
		{
			AwaitEndPing();
		}
		else
		{
			AbortCooldown = true;
			IsActivePing = false;
		}
	}

	protected IEnumerator Cooldown()
	{
		AbortCooldown = false;
		float time = 0.2f;
		while (time > 0f)
		{
			if (AbortCooldown)
			{
				yield break;
			}
			time -= Time.unscaledDeltaTime;
			yield return null;
		}
		AwaitEndPing();
	}

	protected void AwaitEndPing()
	{
		if (IsActivePing)
		{
			IsActivePing = false;
			InternalPing(active: false);
		}
	}

	protected void InternalPing(bool active)
	{
		if (m_ActionComponents != null)
		{
			IPingAction[] actionComponents = m_ActionComponents;
			for (int i = 0; i < actionComponents.Length; i++)
			{
				actionComponents[i]?.OnCallbackPingActive(active);
			}
		}
	}
}
