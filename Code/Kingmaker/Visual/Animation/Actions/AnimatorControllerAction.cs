using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Actions;

public class AnimatorControllerAction : AnimationActionBase
{
	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public RuntimeAnimatorController AnimatorController;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => Enumerable.Empty<AnimationClipWrapper>();

	public override void OnStart(AnimationActionHandle handle)
	{
		handle.StartController(AnimatorController);
	}

	public override void OnUpdate(AnimationActionHandle handle, float deltaTime)
	{
	}

	public override void OnFinish(AnimationActionHandle handle)
	{
	}
}
