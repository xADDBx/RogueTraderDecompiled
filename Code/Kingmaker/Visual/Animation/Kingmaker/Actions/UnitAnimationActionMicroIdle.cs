using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationMicroIdle", menuName = "Animation Manager/Actions/Unit MicroIdle")]
public class UnitAnimationActionMicroIdle : UnitAnimationAction
{
	[Serializable]
	public class ClipList
	{
		public WeaponAnimationStyle Weapon;

		[ValidateNoNullEntries]
		public AnimationClipWrapper[] ClipWrappers = new AnimationClipWrapper[0];
	}

	[Serializable]
	public class MaskedClipList
	{
		public WeaponAnimationStyle MainWeapon;

		public WeaponAnimationStyle OffWeapon;

		[ValidateNotNull]
		public AvatarMask Mask;

		[ValidateNoNullEntries]
		public AnimationClipWrapper[] ClipWrappers = new AnimationClipWrapper[0];
	}

	[SerializeField]
	private bool m_ForDollRoom;

	public TimedProbabilityCurve TriggerProbability;

	public List<ClipList> Variants = new List<ClipList>();

	public MaskedClipList[] OffHandAnimations;

	private List<AnimationClipWrapper> m_ClipWrappersCache;

	public override UnitAnimationType Type
	{
		get
		{
			if (!m_ForDollRoom)
			{
				return UnitAnimationType.MicroIdle;
			}
			return UnitAnimationType.DollRoomMicroIdle;
		}
	}

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_ClipWrappersCache ?? (m_ClipWrappersCache = Variants.EmptyIfNull().SelectMany((ClipList v) => v.ClipWrappers).Distinct()
		.ToList());

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		ClipList clipList = Variants.EmptyIfNull().FirstOrDefault((ClipList v) => v.Weapon == handle.Manager.ActiveMainHandWeaponStyle);
		clipList = clipList ?? Variants.EmptyIfNull().FirstOrDefault((ClipList v) => v.Weapon == WeaponAnimationStyle.None);
		if (clipList == null || clipList.ClipWrappers.Length == 0)
		{
			handle.Variant = -1;
			handle.Release();
			return;
		}
		handle.Variant = handle.Manager.StatefulRandom.Range(0, clipList.ClipWrappers.Length);
		AnimationClipWrapper animationClipWrapper = clipList.ClipWrappers[handle.Variant];
		if (animationClipWrapper == null)
		{
			handle.Variant = -1;
			handle.Release();
			return;
		}
		MaskedClipList maskedClipList = OffHandAnimations.SingleOrDefault((MaskedClipList v) => v.MainWeapon == handle.Manager.ActiveMainHandWeaponStyle && v.OffWeapon == handle.Manager.ActiveOffHandWeaponStyle);
		if (maskedClipList?.ClipWrappers != null && maskedClipList.ClipWrappers.Length > handle.Variant)
		{
			AnimationClipWrapper clipWrapper = maskedClipList.ClipWrappers[handle.Variant];
			handle.Manager.AddAnimationClip(handle, animationClipWrapper, null, useEmptyAvatarMask: true, isAdditive: false, ClipDurationType.Oneshot, new AnimationComposition(handle));
			handle.Manager.AddClipToComposition(handle, clipWrapper, maskedClipList.Mask, isAdditive: false);
		}
		else
		{
			handle.StartClip(animationClipWrapper);
		}
	}
}
