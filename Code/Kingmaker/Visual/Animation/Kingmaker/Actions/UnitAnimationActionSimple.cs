using System.Collections.Generic;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationSimple", menuName = "Animation Manager/Actions/Unit Animation Simple")]
public class UnitAnimationActionSimple : UnitAnimationAction
{
	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	[ValidateNotNull]
	public AnimationClipWrapper ClipWrapper;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if (m_ClipWrappersHashSet != null)
			{
				return m_ClipWrappersHashSet;
			}
			m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper> { ClipWrapper };
			return m_ClipWrappersHashSet;
		}
	}

	public override UnitAnimationType Type => UnitAnimationType.Hit;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.StartClip(ClipWrapper, ClipDurationType.Oneshot);
	}
}
