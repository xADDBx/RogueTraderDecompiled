using System;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventSoundWithPrefix : AnimationClipEventSound
{
	public AnimationClipEventSoundWithPrefix()
	{
	}

	public AnimationClipEventSoundWithPrefix(float time)
		: this(time, isLooped: false, null, 1f)
	{
	}

	public AnimationClipEventSoundWithPrefix(float time, bool isLooped, string name, float volume)
		: base(time, isLooped, name, null, volume)
	{
	}

	public override Action Start(AnimationManager animationManager)
	{
		uint uniqueId = animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostEventWithPrefix(base.Name, base.Volume);
		if (!base.IsLooped)
		{
			return null;
		}
		return delegate
		{
			animationManager.GetComponent<UnitAnimationCallbackReceiver>().StopPlayingById(uniqueId);
		};
	}

	public override object Clone()
	{
		return new AnimationClipEventSoundWithPrefix(base.Time, base.IsLooped, base.Name, base.Volume);
	}

	public override string ToString()
	{
		return $"{base.Name} sound event with prefix at {base.Time}";
	}
}
