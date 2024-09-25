using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Sound;

internal class TriggerAreaLoaded : TriggerObjectBase, IAudioSceneHandler, ISubscriber
{
	public TriggerAreaLoaded(AkAudioTriggerable owner)
		: base(owner, TriggerType.AreaLoad)
	{
	}

	public override void Enable()
	{
		EventBus.Subscribe(this);
	}

	public override void Disable()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnAudioReloaded()
	{
		Trigger();
	}
}
