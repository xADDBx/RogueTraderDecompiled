using System;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventBodyFall : AnimationClipEventSound
{
	public AnimationClipEventBodyFall()
	{
	}

	public AnimationClipEventBodyFall(float time)
		: this(time, null)
	{
	}

	public AnimationClipEventBodyFall(float time, string name)
		: base(time, isLooped: false, name, null, 1f)
	{
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().PlayBodyFall(base.Name);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventBodyFall(base.Time, base.Name);
	}

	public override string ToString()
	{
		return $"{base.Name} body fall event at {base.Time}";
	}
}
