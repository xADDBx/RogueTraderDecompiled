namespace Kingmaker.Sound;

internal class TriggerObjectBase
{
	private readonly AkAudioTriggerable m_Owner;

	public TriggerType Type { get; }

	protected AkAudioTriggerable Owner => m_Owner;

	protected TriggerObjectBase(AkAudioTriggerable owner, TriggerType type)
	{
		m_Owner = owner;
		Type = type;
	}

	public virtual void Disable()
	{
	}

	public virtual void Enable()
	{
	}

	protected void Trigger()
	{
		Owner.TriggerAndLog(this);
	}
}
