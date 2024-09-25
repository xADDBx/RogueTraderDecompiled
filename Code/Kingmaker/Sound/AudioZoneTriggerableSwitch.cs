using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.Sound;

public class AudioZoneTriggerableSwitch : MonoBehaviour, IAbilitySoundZoneTrigger, ISubscriber
{
	public static HashSet<AudioZoneTriggerableSwitch> AudioZone = new HashSet<AudioZoneTriggerableSwitch>();

	[SerializeField]
	private bool m_SelectedEvents;

	[SerializeField]
	private Bounds m_Bounds;

	[SerializeField]
	private AkSwitchReference m_Switch;

	public Bounds Bounds => m_Bounds;

	public AkSwitchReference Switch => m_Switch;

	protected void OnEnable()
	{
		AudioZone.Add(this);
		EventBus.Subscribe(this);
	}

	protected void OnDisable()
	{
		AudioZone.Remove(this);
		EventBus.Unsubscribe(this);
	}

	public void TriggerSoundZone(MechanicsContext context, GameObject go)
	{
		Bounds bounds = new Bounds(Bounds.center + base.transform.position, Bounds.size);
		if (bounds.Contains(go.transform.position) && !m_SelectedEvents)
		{
			m_Switch.Set(base.gameObject);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = new Color(0.81f, 1f, 0.19f);
		Gizmos.DrawWireCube(Bounds.center, Bounds.size);
	}
}
