namespace Kingmaker.Sound;

internal class TriggerDisabled : TriggerObjectBase
{
	public TriggerDisabled(AkAudioTriggerable owner)
		: base(owner, TriggerType.Disabled)
	{
	}

	public override void Disable()
	{
		Trigger();
	}
}
