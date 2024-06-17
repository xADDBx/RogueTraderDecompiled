using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimatioHandEquip", menuName = "Animation Manager/Actions/Unit Hand Equip|Unequip")]
public class UnitAnimationActionStun : UnitAnimationAction
{
	[Serializable]
	public class AnimationEntry
	{
		[AssetPicker("")]
		[SerializeField]
		[ValidateNotNull]
		public AnimationClipWrapper StunnedLoopWrapper;

		[AssetPicker("")]
		[SerializeField]
		public AnimationClipWrapper ExitStunWrapper;

		[SerializeField]
		public WeaponAnimationStyle Style;

		public bool IsValid()
		{
			return StunnedLoopWrapper;
		}
	}

	private class HandleData
	{
		public PlayableInfo FullBody;

		public bool IsExiting;
	}

	[SerializeField]
	private List<AnimationEntry> m_Animations;

	public override UnitAnimationType Type => UnitAnimationType.Stunned;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			foreach (AnimationEntry animation in m_Animations)
			{
				if ((bool)animation.StunnedLoopWrapper)
				{
					yield return animation.StunnedLoopWrapper;
				}
				if ((bool)animation.ExitStunWrapper)
				{
					yield return animation.ExitStunWrapper;
				}
			}
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationEntry animation = GetAnimation(handle);
		if ((bool)animation?.StunnedLoopWrapper)
		{
			handle.StartClip(animation.StunnedLoopWrapper, ClipDurationType.Endless);
			handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
			HandleData actionData = new HandleData
			{
				FullBody = (PlayableInfo)handle.ActiveAnimation.Find(null, isAdditive: false)
			};
			handle.ActionData = actionData;
		}
		handle.Manager.CurrentEquipHandle = handle;
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		base.OnUpdate(handle, deltaTime);
		(handle.ActionData as HandleData)?.FullBody.SetWeightMultiplier((!(handle.Manager.Speed > 0f)) ? 1 : 0);
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		base.OnFinish(handle);
		handle.Manager.CurrentEquipHandle = null;
	}

	public void SwitchToExit(UnitAnimationActionHandle handle)
	{
		if (handle.ActionData == null)
		{
			handle.Release();
			return;
		}
		HandleData handleData = handle.ActionData as HandleData;
		if (!handleData.IsExiting)
		{
			handleData.IsExiting = true;
			AnimationEntry animation = GetAnimation(handle);
			if ((bool)animation?.ExitStunWrapper)
			{
				handle.StartClip(animation?.ExitStunWrapper, ClipDurationType.Oneshot);
				handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
				handleData.FullBody = (PlayableInfo)handle.ActiveAnimation.Find(null, isAdditive: false);
				OnUpdate(handle, 0f);
			}
			else
			{
				handle.Release();
			}
		}
	}

	[CanBeNull]
	private AnimationEntry GetAnimation(UnitAnimationActionHandle handle)
	{
		AnimationEntry animationEntry = null;
		UnitEntityView unitEntityView = handle.Unit as UnitEntityView;
		WeaponAnimationStyle weaponAnimationStyle = ((unitEntityView != null) ? (unitEntityView.HandsEquipment?.ActiveMainHandWeaponStyle ?? WeaponAnimationStyle.None) : WeaponAnimationStyle.None);
		foreach (AnimationEntry animation in m_Animations)
		{
			if (animation.IsValid() && ((animationEntry == null && animation.Style == WeaponAnimationStyle.None) || animation.Style == weaponAnimationStyle))
			{
				animationEntry = animation;
			}
		}
		return animationEntry;
	}

	public void AddAnimations(AnimationClipWrapper loopClip, AnimationClipWrapper outClip)
	{
	}
}
