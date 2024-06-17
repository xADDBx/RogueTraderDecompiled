using System.Collections;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.View;
using Kingmaker.Visual.HitSystem;
using UnityEngine;

namespace Kingmaker;

public class RagdollRecieverMain : MonoBehaviour
{
	private AkSwitchReference testAkRef;

	private AkSwitchReference testAkRef02;

	public float TimerWait = 0.5f;

	public float Impulse01 = 5f;

	public float Impulse02 = 10f;

	private float previousImpulse;

	public UnitEntityView _UnitEntityView;

	private Coroutine m_CurrentDelayedStop;

	private bool firstLaunch = true;

	private string SoundString = "BodyfallsRagDoll_Play";

	private void Awake()
	{
		FindRagdollSender();
	}

	public void FindRagdollSender()
	{
		RagdollSender[] componentsInChildren = GetComponentsInChildren<RagdollSender>();
		RagdollSender[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Receiver = this;
		}
		_UnitEntityView = GetComponentInParent<UnitEntityView>();
		if (componentsInChildren.Length == 0 && _UnitEntityView != null)
		{
			PFLog.TechArt.Log("No Ragdoll sender scripts! Rebake prefab:" + _UnitEntityView.name);
		}
	}

	public void Send(string _name, float _value, SurfaceType? _surface)
	{
		if (firstLaunch)
		{
			firstLaunch = false;
			m_CurrentDelayedStop = StartCoroutine(DelayedStop(TimerWait));
		}
		if (_value > Impulse01 && m_CurrentDelayedStop == null && _UnitEntityView.EntityData.Health.LastHandledDamage != null)
		{
			previousImpulse = _value;
			if (_UnitEntityView != null)
			{
				testAkRef = _UnitEntityView.Blueprint.VisualSettings.BodySizeSoundSwitch;
				testAkRef02 = _UnitEntityView.Blueprint.VisualSettings.BodyTypeSoundSwitch;
				testAkRef.Set(base.gameObject);
				testAkRef02.Set(base.gameObject);
			}
			if (_surface.HasValue)
			{
				AkSoundEngine.SetSwitch("Terrain", _surface.ToString(), base.gameObject);
			}
			SoundEventsManager.PostEvent(SoundString, base.gameObject);
			m_CurrentDelayedStop = StartCoroutine(DelayedStop(TimerWait));
		}
	}

	private IEnumerator DelayedStop(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_CurrentDelayedStop = null;
	}
}
