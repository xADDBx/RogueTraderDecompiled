using System;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventSoundSurface : AnimationClipEventSound
{
	public AnimationClipEventSoundSurface()
	{
	}

	public AnimationClipEventSoundSurface(float time)
		: this(time, null)
	{
	}

	public AnimationClipEventSoundSurface(float time, string name)
		: base(time, isLooped: false, name, null, 1f)
	{
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostEventWithSurface(base.Name);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventSoundSurface(base.Time, base.Name);
	}

	public override string ToString()
	{
		return $"{base.Name} surface sound event at {base.Time}";
	}
}
