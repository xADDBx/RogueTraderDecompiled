namespace Kingmaker.Sound;

internal class TriggerEnabled : TriggerObjectBase
{
	public TriggerEnabled(AkAudioTriggerable owner)
		: base(owner, TriggerType.Enabled)
	{
	}

	public override void Enable()
	{
		Trigger();
	}
}
