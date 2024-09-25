using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationDodge", menuName = "Animation Manager/Actions/Unit Dodge")]
public class UnitAnimationActionDodge : UnitAnimationAction
{
	[Serializable]
	public class WeaponStyleSettings
	{
		public WeaponAnimationStyle Style;

		[AssetPicker("")]
		[ValidateNotNull]
		[ValidateHasActEvent]
		public AnimationClipWrapper Clip;

		public AnimationClipWrapper[] RandomClips = new AnimationClipWrapper[1];

		public AnimationClipWrapper DodgeInFullCover;

		public AnimationClipWrapper DodgeInHalfCover;

		public IEnumerable<AnimationClipWrapper> GetClips()
		{
			List<AnimationClipWrapper> list = RandomClips.ToList();
			list.Add(Clip);
			list.Add(DodgeInFullCover);
			list.Add(DodgeInHalfCover);
			return list;
		}
	}

	[SerializeField]
	private WeaponStyleSettings[] m_Settings = new WeaponStyleSettings[1];

	public IReadOnlyList<WeaponStyleSettings> Settings => m_Settings;

	public override bool IsAdditive => true;

	public override bool IsAdditiveToItself => false;

	public override UnitAnimationType Type => UnitAnimationType.Dodge;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Settings.SelectMany((WeaponStyleSettings i) => i.GetClips());

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper variant = GetVariant(handle);
		if ((bool)variant)
		{
			handle.StartClip(variant, ClipDurationType.Oneshot);
		}
		else
		{
			handle.Release();
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
	}

	private AnimationClipWrapper GetVariant(UnitAnimationActionHandle handle)
	{
		WeaponStyleSettings weaponStyleSettings = m_Settings.FirstOrDefault((WeaponStyleSettings i) => i.Style == handle.Manager.ActiveMainHandWeaponStyle) ?? m_Settings.FirstOrDefault((WeaponStyleSettings i) => i.Style == WeaponAnimationStyle.None);
		if (handle.Manager.InCover && handle.Manager.CoverType == LosCalculations.CoverType.Full && weaponStyleSettings?.DodgeInFullCover != null)
		{
			return weaponStyleSettings.DodgeInFullCover;
		}
		if (handle.Manager.InCover && handle.Manager.CoverType == LosCalculations.CoverType.Half && weaponStyleSettings?.DodgeInHalfCover != null)
		{
			return weaponStyleSettings.DodgeInHalfCover;
		}
		if (weaponStyleSettings == null || weaponStyleSettings.RandomClips.Length == 0)
		{
			return weaponStyleSettings?.Clip;
		}
		object obj = ((weaponStyleSettings != null) ? weaponStyleSettings.RandomClips[handle.Manager.StatefulRandom.Range(0, weaponStyleSettings.RandomClips.Length)] : null);
		if (obj == null)
		{
			if (weaponStyleSettings == null)
			{
				return null;
			}
			obj = weaponStyleSettings.Clip;
		}
		return (AnimationClipWrapper)obj;
	}
}
