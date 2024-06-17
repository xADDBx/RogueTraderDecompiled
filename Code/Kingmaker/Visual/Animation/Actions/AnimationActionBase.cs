using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Actions;

public abstract class AnimationActionBase : ScriptableObject
{
	public float TransitionIn = 0.2f;

	public float TransitionOut = 0.2f;

	public ExecutionMode ExecutionMode;

	public bool UseEmptyAvatarMask = true;

	public List<AvatarMask> AvatarMasks;

	public virtual bool IsAdditive => false;

	public virtual bool DontReleaseOnInterrupt => false;

	public virtual bool IsAdditiveToItself => true;

	public virtual bool IsAdditiveInterruptsSameType => false;

	public abstract IEnumerable<AnimationClipWrapper> ClipWrappers { get; }

	public virtual bool SupportCaching => true;

	public virtual bool ForceFinishOnJoinCombat => false;

	public abstract void OnStart(AnimationActionHandle handle);

	public abstract void OnFinish(AnimationActionHandle handle);

	public abstract void OnUpdate(AnimationActionHandle handle, float deltaTime);

	public virtual void OnTransitionOutStarted(AnimationActionHandle handle)
	{
		handle.Release();
	}

	public virtual void OnSequencedInterrupted(AnimationActionHandle handle)
	{
	}
}
