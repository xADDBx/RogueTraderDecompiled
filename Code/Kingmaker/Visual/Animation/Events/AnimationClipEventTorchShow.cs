using System;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventTorchShow : AnimationClipEvent
{
	public AnimationClipEventTorchShow(float time)
		: base(time)
	{
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().UnhideTorchEvent();
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventTorchShow(base.Time);
	}

	public override string ToString()
	{
		return $"Torch show event at {base.Time}";
	}
}
