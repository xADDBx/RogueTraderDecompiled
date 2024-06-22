using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerBuffLoopAction", menuName = "Animation Manager/Actions/Buff Loop Animation ")]
public class WarhammerBuffLoopAction : UnitAnimationAction
{
	private enum State
	{
		Start,
		Loop,
		End
	}

	[Serializable]
	public class AnimationEntry
	{
		[AssetPicker("")]
		[SerializeField]
		[ValidateNotNull]
		public AnimationClipWrapper LoopWrapper;

		[AssetPicker("")]
		[SerializeField]
		public AnimationClipWrapper EnterWrapper;

		[AssetPicker("")]
		[SerializeField]
		public AnimationClipWrapper ExitWrapper;

		[SerializeField]
		public WeaponAnimationStyle Style;

		public bool IsOffHand;

		public bool IsValid()
		{
			return LoopWrapper;
		}
	}

	private class HandleData
	{
		public State State;
	}

	[SerializeField]
	private List<AnimationEntry> m_Animations;

	public AvatarMask OffHandMask;

	public override UnitAnimationType Type => UnitAnimationType.BuffLoopAction;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			foreach (AnimationEntry animation in m_Animations)
			{
				if ((bool)animation.LoopWrapper)
				{
					yield return animation.LoopWrapper;
				}
				if ((bool)animation.ExitWrapper)
				{
					yield return animation.ExitWrapper;
				}
				if ((bool)animation.EnterWrapper)
				{
					yield return animation.EnterWrapper;
				}
			}
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationEntry animation = GetAnimation(handle);
		if ((bool)animation?.EnterWrapper)
		{
			handle.StartClip(animation.EnterWrapper, ClipDurationType.Oneshot);
			handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
			HandleData actionData = new HandleData
			{
				State = State.Start
			};
			handle.ActionData = actionData;
		}
		handle.Manager.CurrentEquipHandle = handle;
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (!(handle.ActionData is HandleData { State: var state } handleData))
		{
			handle.Release();
			return;
		}
		if (state == State.Start || state == State.Loop)
		{
			AnimationEntry animation = GetAnimation(handle);
			if ((bool)animation?.LoopWrapper)
			{
				handle.StartClip(animation.LoopWrapper, ClipDurationType.Oneshot);
			}
			else
			{
				handle.Release();
			}
		}
		if (handleData.State == State.End)
		{
			handle.Release();
		}
	}

	public void SwitchToExit(UnitAnimationActionHandle handle)
	{
		if (handle.ActionData == null)
		{
			handle.Release();
			return;
		}
		HandleData handleData = handle.ActionData as HandleData;
		if (handleData.State != State.End)
		{
			handleData.State = State.End;
			AnimationEntry animation = GetAnimation(handle);
			if ((bool)animation?.ExitWrapper)
			{
				handle.StartClip(animation?.ExitWrapper, ClipDurationType.Oneshot);
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
		WeaponAnimationStyle weaponAnimationStyle = ((handle.Unit is UnitEntityView unitEntityView) ? (unitEntityView.HandsEquipment?.ActiveMainHandWeaponStyle ?? WeaponAnimationStyle.None) : WeaponAnimationStyle.None);
		foreach (AnimationEntry animation in m_Animations)
		{
			if (animation.IsValid() && ((animationEntry == null && animation.Style == WeaponAnimationStyle.None) || (animation.Style == weaponAnimationStyle && !animation.IsOffHand)))
			{
				animationEntry = animation;
			}
		}
		if (animationEntry == null)
		{
			animationEntry = m_Animations.FirstOrDefault();
		}
		return animationEntry;
	}

	private AnimationEntry GetAnimationOffHand(UnitAnimationActionHandle handle)
	{
		AnimationEntry animationEntry = null;
		WeaponAnimationStyle weaponAnimationStyle = ((handle.Unit is UnitEntityView unitEntityView) ? (unitEntityView.HandsEquipment?.ActiveOffHandWeaponStyle ?? WeaponAnimationStyle.None) : WeaponAnimationStyle.None);
		foreach (AnimationEntry animation in m_Animations)
		{
			if (animation.IsValid() && ((animationEntry == null && animation.Style == WeaponAnimationStyle.None) || (animation.Style == weaponAnimationStyle && animation.IsOffHand)))
			{
				animationEntry = animation;
			}
		}
		if (animationEntry == null)
		{
			animationEntry = m_Animations.FirstOrDefault();
		}
		return animationEntry;
	}
}
