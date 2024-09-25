using Kingmaker.Utility.CodeTimer;
using UnityEngine;
using UnityEngine.Animations;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipWrapperStateMachineBehaviour : StateMachineBehaviour
{
	public AnimationClipWrapper AnimationClipWrapper;

	private AnimationManager m_AnimationManagerCached;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		using (ProfileScope.New("AnimationClipWrapperStateMachineBehaviour.OnStateEnter"))
		{
			if (!(AnimationClipWrapper == null))
			{
				m_AnimationManagerCached = m_AnimationManagerCached ?? (m_AnimationManagerCached = animator.GetComponent<AnimationManager>());
				m_AnimationManagerCached.StartEvents(controller, AnimationClipWrapper.EventsSorted);
			}
		}
	}
}
