using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationActionHit", menuName = "Animation Manager/Actions/Unit Hit")]
public class UnitAnimationActionHit : UnitAnimationAction
{
	[Serializable]
	public class WeaponStyleOverride
	{
		public WeaponAnimationStyle WeaponAnimationStyle;

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

	public override UnitAnimationType Type => UnitAnimationType.Hit;

	public override bool IsAdditive => false;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		if (handle.Manager.IsDead)
		{
			return;
		}
		AnimationClipWrapper clipWrapper = ClipWrapper;
		WeaponStyleOverride weaponStyleOverride = WeaponStyleOverrides.FirstItem((WeaponStyleOverride x) => x.WeaponAnimationStyle == handle.Manager.ActiveMainHandWeaponStyle);
		if (weaponStyleOverride != null && weaponStyleOverride.ClipWrapper != null)
		{
			if (handle.Manager.InCover)
			{
				if (handle.Manager.CoverType == LosCalculations.CoverType.Full && weaponStyleOverride.HitInFullCover.Length != 0)
				{
					handle.StartClip(weaponStyleOverride.HitInFullCover[Mathf.RoundToInt(handle.Manager.StatefulRandom.Range(0, weaponStyleOverride.HitInFullCover.Length - 1))] ?? weaponStyleOverride.ClipWrapper, ClipDurationType.Oneshot);
					return;
				}
				if (handle.Manager.CoverType == LosCalculations.CoverType.Half && weaponStyleOverride.HitInHalfCover.Length != 0)
				{
					handle.StartClip(weaponStyleOverride.HitInHalfCover[Mathf.RoundToInt(handle.Manager.StatefulRandom.Range(0, weaponStyleOverride.HitInHalfCover.Length - 1))] ?? weaponStyleOverride.ClipWrapper, ClipDurationType.Oneshot);
					return;
				}
			}
			clipWrapper = ((weaponStyleOverride.RandomClips.Length != 0) ? (weaponStyleOverride.RandomClips[Mathf.RoundToInt(handle.Manager.StatefulRandom.Range(0, weaponStyleOverride.RandomClips.Length - 1))] ?? weaponStyleOverride.ClipWrapper) : weaponStyleOverride.ClipWrapper);
			handle.StartClip(clipWrapper, ClipDurationType.Oneshot);
			return;
		}
		if (handle.Manager.InCover)
		{
			if (handle.Manager.CoverType == LosCalculations.CoverType.Full && HitInFullCover.Length != 0)
			{
				handle.StartClip(HitInFullCover[Mathf.RoundToInt(handle.Manager.StatefulRandom.Range(0, HitInFullCover.Length - 1))] ?? ClipWrapper, ClipDurationType.Oneshot);
				return;
			}
			if (handle.Manager.CoverType == LosCalculations.CoverType.Half && HitInHalfCover.Length != 0)
			{
				handle.StartClip(HitInHalfCover[Mathf.RoundToInt(handle.Manager.StatefulRandom.Range(0, HitInHalfCover.Length - 1))] ?? ClipWrapper, ClipDurationType.Oneshot);
				return;
			}
		}
		clipWrapper = ((RandomClips.Length != 0) ? (RandomClips[Mathf.RoundToInt(handle.Manager.StatefulRandom.Range(0, RandomClips.Length - 1))] ?? ClipWrapper) : ClipWrapper);
		handle.StartClip(clipWrapper, ClipDurationType.Oneshot);
	}
}
