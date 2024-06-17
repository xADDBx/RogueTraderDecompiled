using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Sound;

internal class TriggerZone : TriggerObjectBase, IAudioZoneHandler, ISubscriber
{
	private readonly bool m_OnEnter;

	private readonly AudioZone m_ParentZone;

	private bool m_IsInsideNow;

	public TriggerZone(AkAudioTriggerable owner, bool onEnter)
		: base(owner, onEnter ? TriggerType.ZoneEntered : TriggerType.ZoneExited)
	{
		m_OnEnter = onEnter;
		m_ParentZone = owner.GetComponentInParent<AudioZone>();
	}

	public override void Enable()
	{
		EventBus.Subscribe(this);
		if ((bool)m_ParentZone && m_ParentZone.ListenerInside && m_OnEnter)
		{
			Trigger();
		}
	}

	public override void Disable()
	{
		EventBus.Unsubscribe(this);
		if ((bool)m_ParentZone && m_ParentZone.ListenerInside && !m_OnEnter)
		{
			Trigger();
		}
	}

	public void HandleListenerZone(AudioZone zone, bool isInside)
	{
		if (m_ParentZone == zone && isInside == m_OnEnter)
		{
			Trigger();
		}
	}
}
