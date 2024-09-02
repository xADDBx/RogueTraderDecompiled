using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.Attributes;
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

	public override bool BlocksCover => true;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.SkipFirstTick = false;
		handle.HasCrossfadePriority = true;
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
		if (!(handle.ActionData is HandleData handleData) || handle.IsReleased)
		{
			handle.Release();
			return;
		}
		State state = handleData.State;
		if (state == State.Start || state == State.Loop)
		{
			AnimationEntry animation = GetAnimation(handle);
			if ((bool)animation?.LoopWrapper)
			{
				AnimationEntry animation2 = GetAnimation(handle, isOffHand: true);
				if (animation2 != null)
				{
					handle.Manager.AddAnimationClip(handle, animation.LoopWrapper, null, useEmptyAvatarMask: true, isAdditive: false, ClipDurationType.Oneshot, new AnimationComposition(handle));
					handle.Manager.AddClipToComposition(handle, animation2.LoopWrapper, OffHandMask, isAdditive: false);
				}
				else
				{
					handle.StartClip(animation.LoopWrapper, ClipDurationType.Oneshot);
				}
				handleData.State = State.Loop;
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
		if (!(handle.ActionData is HandleData handleData))
		{
			handle.Release();
		}
		else if (handleData.State != State.End)
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
	private AnimationEntry GetAnimation(UnitAnimationActionHandle handle, bool isOffHand = false)
	{
		if (!(handle.Unit is UnitEntityView unitEntityView))
		{
			return null;
		}
		WeaponAnimationStyle valueOrDefault = ((!isOffHand) ? unitEntityView.HandsEquipment?.ActiveMainHandWeaponStyle : unitEntityView.HandsEquipment?.ActiveOffHandWeaponStyle).GetValueOrDefault();
		AnimationEntry animationEntry = null;
		foreach (AnimationEntry animation in m_Animations)
		{
			if (animation.IsValid() && animation.IsOffHand == isOffHand && (animation.Style == valueOrDefault || (animationEntry == null && animation.Style == WeaponAnimationStyle.None)))
			{
				animationEntry = animation;
			}
		}
		return animationEntry;
	}
}
