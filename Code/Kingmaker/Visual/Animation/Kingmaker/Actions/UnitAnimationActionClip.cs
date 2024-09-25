using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Visual.Animation.Actions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationClip", menuName = "Animation Manager/Actions/Unit Untyped Clip")]
public class UnitAnimationActionClip : UnitAnimationAction
{
	private bool m_IsTransient;

	[ValidateNotNull]
	public AnimationClipWrapper m_Clip;

	public override UnitAnimationType Type => UnitAnimationType.None;

	public override bool SupportCaching => Type != UnitAnimationType.None;

	public override bool ForceFinishOnJoinCombat => true;

	public bool Looping
	{
		get
		{
			if (m_Clip != null)
			{
				return m_Clip.IsLooping;
			}
			return false;
		}
	}

	public ClipDurationType DurationType { get; set; }

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			yield return m_Clip;
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		if (m_Clip == null)
		{
			handle.Release();
		}
		else
		{
			handle.StartClip(m_Clip, DurationType);
		}
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		base.OnFinish(handle);
		if (m_IsTransient)
		{
			Object.Destroy(this);
		}
	}

	public override void OnSequencedInterrupted(AnimationActionHandle handle)
	{
		base.OnSequencedInterrupted(handle);
		if (m_IsTransient)
		{
			Object.Destroy(this);
		}
	}

	public static UnitAnimationActionClip Create(AnimationClipWrapper clip, [CallerMemberName] string callerName = "")
	{
		UnitAnimationActionClip unitAnimationActionClip = ScriptableObject.CreateInstance<UnitAnimationActionClip>();
		unitAnimationActionClip.name = "Clip: " + clip.name + "/" + callerName;
		unitAnimationActionClip.m_Clip = clip;
		unitAnimationActionClip.TransitionOut = 0.3f;
		unitAnimationActionClip.TransitionIn = 0.3f;
		unitAnimationActionClip.m_IsTransient = true;
		return unitAnimationActionClip;
	}
}
