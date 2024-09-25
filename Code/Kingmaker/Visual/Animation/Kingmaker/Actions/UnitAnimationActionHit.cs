using System;
using System.Collections.Generic;
using Kingmaker.Controllers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using Kingmaker.Visual.Animation.Actions;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationActionHit", menuName = "Animation Manager/Actions/Unit Hit")]
public class UnitAnimationActionHit : UnitAnimationAction
{
	[Serializable]
	public class WeaponStyleOverride
	{
		public WeaponAnimationStyle WeaponAnimationStyle;

		public bool IsOffhand;

		private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

		public AnimationClipWrapper ClipWrapper;

		public AnimationClipWrapper[] RandomClips = new AnimationClipWrapper[0];

		public AnimationClipWrapper[] HitInFullCover = new AnimationClipWrapper[0];

		public AnimationClipWrapper[] HitInHalfCover = new AnimationClipWrapper[0];

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				if (m_ClipWrappersHashSet != null)
				{
					return m_ClipWrappersHashSet;
				}
				m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper> { ClipWrapper };
				m_ClipWrappersHashSet.AddRange(RandomClips);
				m_ClipWrappersHashSet.AddRange(HitInFullCover);
				m_ClipWrappersHashSet.AddRange(HitInHalfCover);
				return m_ClipWrappersHashSet;
			}
		}
	}

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public AnimationClipWrapper ClipWrapper;

	public AnimationClipWrapper[] RandomClips = new AnimationClipWrapper[0];

	public AnimationClipWrapper[] HitInFullCover = new AnimationClipWrapper[0];

	public AnimationClipWrapper[] HitInHalfCover = new AnimationClipWrapper[0];

	public WeaponStyleOverride[] WeaponStyleOverrides = new WeaponStyleOverride[0];

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if (m_ClipWrappersHashSet != null)
			{
				return m_ClipWrappersHashSet;
			}
			m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper> { ClipWrapper };
			m_ClipWrappersHashSet.AddRange(RandomClips);
			m_ClipWrappersHashSet.AddRange(HitInFullCover);
			m_ClipWrappersHashSet.AddRange(HitInHalfCover);
			WeaponStyleOverride[] weaponStyleOverrides = WeaponStyleOverrides;
			foreach (WeaponStyleOverride weaponStyleOverride in weaponStyleOverrides)
			{
				m_ClipWrappersHashSet.AddRange(weaponStyleOverride.ClipWrappers);
			}
			return m_ClipWrappersHashSet;
		}
	}

	public static float CrossfadeToCoverTime => RealTimeController.SystemStepDurationSeconds * 2f;

	public override UnitAnimationType Type => UnitAnimationType.Hit;

	public override bool IsAdditive => false;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		if (!handle.Manager.IsDead)
		{
			handle.SkipFirstTick = false;
			handle.CorrectTransitionOutTime = true;
			handle.StartClip(GetAnimationClip(handle), ClipDurationType.Oneshot);
			if (handle.Manager.InCover && handle.ActiveAnimation != null)
			{
				handle.ActiveAnimation.ChangeTransitionTime(CrossfadeToCoverTime + RealTimeController.SystemStepDurationSeconds);
			}
		}
	}

	private AnimationClipWrapper GetAnimationClip(UnitAnimationActionHandle handle)
	{
		bool isOffHand = handle.Manager.ActiveMainHandWeaponStyle == WeaponAnimationStyle.None;
		WeaponStyleOverride weaponStyleOverride = WeaponStyleOverrides.FirstItem((WeaponStyleOverride x) => x.WeaponAnimationStyle == handle.Manager.ActiveMainHandWeaponStyle && x.IsOffhand == isOffHand) ?? WeaponStyleOverrides.FirstItem((WeaponStyleOverride x) => x.WeaponAnimationStyle == handle.Manager.ActiveMainHandWeaponStyle);
		if (weaponStyleOverride != null && weaponStyleOverride.ClipWrapper != null)
		{
			return SelectClipFromCover(weaponStyleOverride.HitInFullCover, weaponStyleOverride.HitInHalfCover, weaponStyleOverride.RandomClips, weaponStyleOverride.ClipWrapper);
		}
		return SelectClipFromCover(HitInFullCover, HitInHalfCover, RandomClips, ClipWrapper);
		AnimationClipWrapper GetRandomClip(AnimationClipWrapper[] randomClips, AnimationClipWrapper defaultClip)
		{
			if (randomClips.Length == 0)
			{
				return defaultClip;
			}
			return randomClips[Mathf.RoundToInt(handle.Manager.StatefulRandom.Range(0, randomClips.Length - 1))] ?? defaultClip;
		}
		AnimationClipWrapper SelectClipFromCover(AnimationClipWrapper[] fullCoverClips, AnimationClipWrapper[] halfCoverClips, AnimationClipWrapper[] randomClips, AnimationClipWrapper defaultClip)
		{
			if (handle.Manager.InCover)
			{
				handle.HasCrossfadePriority = true;
				handle.Manager.HitAnimationIsActive = true;
				if (handle.Manager.CoverType == LosCalculations.CoverType.Full && fullCoverClips.Length != 0)
				{
					return GetRandomClip(fullCoverClips, defaultClip);
				}
				if (handle.Manager.CoverType == LosCalculations.CoverType.Half && halfCoverClips.Length != 0)
				{
					return GetRandomClip(halfCoverClips, defaultClip);
				}
			}
			return GetRandomClip(randomClips, defaultClip);
		}
	}

	public bool BlocksReturnToCover(AnimationActionHandle handle)
	{
		AnimationBase activeAnimation = handle.ActiveAnimation;
		if (activeAnimation == null)
		{
			return false;
		}
		return activeAnimation.GetTime() < activeAnimation.TransitionOutStartTime - RealTimeController.SystemStepDurationSeconds;
	}
}
