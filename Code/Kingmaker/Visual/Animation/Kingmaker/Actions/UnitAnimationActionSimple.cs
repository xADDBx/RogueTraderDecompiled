using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationSimple", menuName = "Animation Manager/Actions/Unit Animation Simple")]
public class UnitAnimationActionSimple : UnitAnimationAction
{
	[Serializable]
	public class WeaponStyleOverride
	{
		public WeaponAnimationStyle WeaponAnimationStyle;

		public AnimationClipWrapper ClipWrapper;
	}

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	[ValidateNotNull]
	public AnimationClipWrapper ClipWrapper;

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
			WeaponStyleOverride[] weaponStyleOverrides = WeaponStyleOverrides;
			foreach (WeaponStyleOverride weaponStyleOverride in weaponStyleOverrides)
			{
				m_ClipWrappersHashSet.Add(weaponStyleOverride.ClipWrapper);
			}
			return m_ClipWrappersHashSet;
		}
	}

	public override UnitAnimationType Type => UnitAnimationType.Hit;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		WeaponStyleOverride weaponStyleOverride = WeaponStyleOverrides.FirstItem((WeaponStyleOverride x) => x.WeaponAnimationStyle == handle.Manager.ActiveMainHandWeaponStyle);
		handle.StartClip(weaponStyleOverride?.ClipWrapper ? weaponStyleOverride.ClipWrapper : ClipWrapper, ClipDurationType.Oneshot);
	}
}
