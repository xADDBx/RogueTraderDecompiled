using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Sound;

internal class TriggerAreaUnloaded : TriggerObjectBase, IAreaHandler, ISubscriber
{
	private bool m_DidTrigger;

	public TriggerAreaUnloaded(AkAudioTriggerable owner)
		: base(owner, TriggerType.AreaUnload)
	{
	}

	public override void Enable()
	{
		EventBus.Subscribe(this);
		m_DidTrigger = false;
	}

	public override void Disable()
	{
		EventBus.Unsubscribe(this);
		if (!m_DidTrigger)
		{
			OnAreaBeginUnloading();
		}
	}

	public void OnAreaBeginUnloading()
	{
		Trigger();
		m_DidTrigger = true;
	}

	public void OnAreaDidLoad()
	{
	}
}
