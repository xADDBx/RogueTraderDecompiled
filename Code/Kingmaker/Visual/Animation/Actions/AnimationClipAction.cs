using System.Collections.Generic;

namespace Kingmaker.Visual.Animation.Actions;

public class AnimationClipAction : AnimationActionBase
{
	public AnimationClipWrapper ClipWrapper;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			yield return ClipWrapper;
		}
	}

	public override void OnStart(AnimationActionHandle handle)
	{
		handle.StartClip(ClipWrapper);
	}

	public override void OnUpdate(AnimationActionHandle handle, float deltaTime)
	{
	}

	public override void OnTransitionOutStarted(AnimationActionHandle handle)
	{
		handle.Release();
	}

	public override void OnFinish(AnimationActionHandle handle)
	{
	}
}
