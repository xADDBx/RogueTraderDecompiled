using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationHandnAttack", menuName = "Animation Manager/Actions/Unit Hand Attack")]
public class UnitAnimationActionHandAttack : UnitAnimationAction, IUnitAnimationActionHasVariants
{
	[Serializable]
	public class WeaponStyleSettings
	{
		public WeaponAnimationStyle Style;

		[AssetPicker("")]
		[ValidateNotEmpty]
		[ValidateNoNullEntries]
		[ValidateHasActEvent]
		[DrawEventWarning]
		public List<AnimationClipWrapper> Single = new List<AnimationClipWrapper>();

		[AssetPicker("")]
		[ValidateNoNullEntries]
		[ValidateHasActEvent]
		[DrawEventWarning]
		public List<AnimationClipWrapper> Burst = new List<AnimationClipWrapper>();

		[AssetPicker("")]
		[ValidateHasActEvent]
		[DrawEventWarning]
		public AnimationClipWrapper CornerShotLeft;

		[AssetPicker("")]
		[ValidateHasActEvent]
		[DrawEventWarning]
		public AnimationClipWrapper CornerShotRight;

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				for (int i = 0; i < Single.Count; i++)
				{
					yield return Single[i];
				}
				for (int i = 0; i < Burst.Count; i++)
				{
					yield return Burst[i];
				}
				if ((bool)CornerShotLeft)
				{
					yield return CornerShotLeft;
				}
				if ((bool)CornerShotRight)
				{
					yield return CornerShotRight;
				}
			}
		}

		public AnimationClipWrapper GetVariant(UnitAnimationActionHandle handle)
		{
			AnimationClipWrapper animationClipWrapper = (handle.AnimationClipWrapper ? handle.AnimationClipWrapper : (handle.IsBurst ? (Burst.Get(handle.Variant) ?? Single.Get(handle.Variant)) : Single.Get(handle.Variant)));
			if (!animationClipWrapper)
			{
				PFLog.Default.Error(handle.Action, $"Action has null variant {handle.Variant} for {Style} in {handle.Action.name}");
				animationClipWrapper = (handle.IsBurst ? (Burst.Get(0) ?? Single.Get(0)) : Single.Get(0));
			}
			if (!animationClipWrapper)
			{
				PFLog.Default.Error(handle.Action, $"Action has no variants for {Style} in {handle.Action.name}");
			}
			return animationClipWrapper;
		}

		public void Validate(ValidationContext context)
		{
			if (Style == WeaponAnimationStyle.None)
			{
				context.AddError("Invalid Style: None");
			}
			if (Single.Any((AnimationClipWrapper i) => i == null))
			{
				context.AddError("Invalid clip wrapper in variants");
			}
			if (Burst.Any((AnimationClipWrapper i) => i == null))
			{
				context.AddError("Invalid clip wrapper in mounted variants");
			}
		}
	}

	[SerializeField]
	private bool m_IsMainHand;

	[SerializeField]
	private List<WeaponStyleSettings> m_Settings = new List<WeaponStyleSettings>();

	public List<WeaponStyleSettings> Settings => m_Settings;

	public override UnitAnimationType Type
	{
		get
		{
			if (!m_IsMainHand)
			{
				return UnitAnimationType.OffHandAttack;
			}
			return UnitAnimationType.MainHandAttack;
		}
	}

	public bool IsMainHand
	{
		get
		{
			return m_IsMainHand;
		}
		set
		{
			m_IsMainHand = value;
		}
	}

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Settings.SelectMany((WeaponStyleSettings i) => i.ClipWrappers);

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper variant = GetVariant(handle);
		if ((bool)variant)
		{
			handle.StartClip(variant, ClipDurationType.Oneshot);
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (handle.ActiveAnimation == null && handle.GetTime() > 0.1f)
		{
			handle.ActEventsCounter++;
			handle.Release();
		}
	}

	private AnimationClipWrapper GetVariant(UnitAnimationActionHandle handle)
	{
		WeaponStyleSettings weaponStyleSettings = m_Settings.FirstOrDefault((WeaponStyleSettings i) => i.Style == handle.AttackWeaponStyle);
		if (weaponStyleSettings == null)
		{
			PFLog.Default.Error(this, $"No animation for weapon style '{handle.AttackWeaponStyle}' in action '{base.name}'");
		}
		return weaponStyleSettings?.GetVariant(handle);
	}

	public int GetVariantsCount(UnitAnimationActionHandle handle)
	{
		WeaponStyleSettings weaponStyleSettings = m_Settings.FirstOrDefault((WeaponStyleSettings i) => i.Style == handle.AttackWeaponStyle);
		if (weaponStyleSettings == null)
		{
			return 0;
		}
		if (handle.IsBurst && weaponStyleSettings.Burst.Any())
		{
			return weaponStyleSettings.Burst.Count;
		}
		return weaponStyleSettings.Single.Count;
	}

	public bool HasSetting(WeaponAnimationStyle style)
	{
		return m_Settings.Any((WeaponStyleSettings s) => s.Style == style && s.Single.Any((AnimationClipWrapper c) => c));
	}

	public void AddToSetting(WeaponAnimationStyle style, AnimationClipWrapper clip)
	{
	}
}
