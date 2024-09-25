using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class RuntimeAnimatorControllerWrapper : ScriptableObject
{
	private AnimationClipWrapper[] m_AnimationClipWrappers;

	public RuntimeAnimatorController RuntimeAnimatorController;

	public AnimationClipWrapper[] AnimationClipWrappers => m_AnimationClipWrappers;
}
