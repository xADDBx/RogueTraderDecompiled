using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers;
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

		public bool IsExiting;
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

	private static float CrossfadeTime => RealTimeController.SystemStepDurationSeconds;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.SkipFirstTick = false;
		handle.SkipFirstTickOnHandle = false;
		handle.CorrectTransitionOutTime = true;
		handle.HasCrossfadePriority = true;
		HandleData handleData2 = (HandleData)(handle.ActionData = new HandleData());
		if (!handle.SkipEnterAnimation && TryPlayAnimation(handle, (AnimationEntry e) => e.EnterWrapper))
		{
			handleData2.State = State.Start;
			handle.ActiveAnimation.ChangeTransitionTime(CrossfadeTime);
		}
		else if (TryPlayAnimation(handle, (AnimationEntry e) => e.LoopWrapper))
		{
			handleData2.State = State.Loop;
			handle.ActiveAnimation.ChangeTransitionTime(CrossfadeTime);
		}
		else
		{
			handleData2.State = State.End;
			handle.Release();
		}
		handle.Manager.CurrentEquipHandle = handle;
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (!(handle.ActionData is HandleData handleData) || handle.IsReleased || handle.IsInterrupted)
		{
			handle.Release();
			if (handle.IsInterrupted && handle.ActiveAnimation != null)
			{
				handle.ActiveAnimation.TransitionOut = handle.Manager.GetTransitionOutDuration(handle);
			}
			return;
		}
		State state = handleData.State;
		if ((state == State.Start || state == State.Loop) && !handleData.IsExiting)
		{
			if (TryPlayAnimation(handle, (AnimationEntry e) => e.LoopWrapper))
			{
				handle.ActiveAnimation.TransitionIn = CrossfadeTime;
				handle.ActiveAnimation.ChangeTransitionTime(CrossfadeTime);
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
			handleData.IsExiting = true;
			float transitionOutDuration = handle.Manager.GetTransitionOutDuration(handle);
			handle.ActiveAnimation.TransitionOut = transitionOutDuration;
			if (TryPlayAnimation(handle, (AnimationEntry e) => e.ExitWrapper))
			{
				handle.ActiveAnimation.TransitionIn = transitionOutDuration;
			}
			else
			{
				handle.Release();
			}
			handleData.State = State.End;
		}
	}

	public bool IsExiting(UnitAnimationActionHandle handle)
	{
		if (handle.ActionData is HandleData handleData)
		{
			return handleData.IsExiting;
		}
		return false;
	}

	private bool TryPlayAnimation(UnitAnimationActionHandle handle, Func<AnimationEntry, AnimationClipWrapper> wrapperGetter)
	{
		AnimationEntry animation = GetAnimation(handle);
		if (animation == null || !wrapperGetter(animation))
		{
			return false;
		}
		AnimationEntry animation2 = GetAnimation(handle, isOffHand: true);
		if (animation2 != null && (bool)wrapperGetter(animation2))
		{
			handle.Manager.AddAnimationClip(handle, wrapperGetter(animation), null, useEmptyAvatarMask: true, isAdditive: false, ClipDurationType.Oneshot, new AnimationComposition(handle));
			handle.Manager.AddClipToComposition(handle, wrapperGetter(animation2), OffHandMask, isAdditive: false);
		}
		else
		{
			handle.StartClip(wrapperGetter(animation), ClipDurationType.Oneshot);
		}
		return true;
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
