using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationBuffState", menuName = "Animation Manager/Actions/Custom/Unit Buff State")]
public class UnitAnimationActionBuffState : UnitAnimationAction
{
	private class HandleData
	{
		public bool IsExiting;
	}

	[AssetPicker("")]
	public AnimationClipWrapper EnterWrapper;

	[AssetPicker("")]
	public AnimationClipWrapper LoopWrapper;

	[AssetPicker("")]
	public AnimationClipWrapper ExitWrapper;

	public override UnitAnimationType Type => UnitAnimationType.Unused;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if ((bool)EnterWrapper)
			{
				yield return EnterWrapper;
			}
			if ((bool)LoopWrapper)
			{
				yield return LoopWrapper;
			}
			if ((bool)ExitWrapper)
			{
				yield return ExitWrapper;
			}
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.ActEventsCounter++;
		handle.ActionData = new HandleData();
		if ((bool)EnterWrapper)
		{
			handle.StartClip(EnterWrapper, ClipDurationType.Oneshot);
		}
		else
		{
			handle.StartClip(LoopWrapper, ClipDurationType.Endless);
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (handle.ActiveAnimation.GetActiveClip() == EnterWrapper.AnimationClip)
		{
			if ((bool)LoopWrapper)
			{
				handle.StartClip(LoopWrapper, ClipDurationType.Endless);
			}
			else if ((bool)ExitWrapper)
			{
				handle.StartClip(ExitWrapper, ClipDurationType.Oneshot);
			}
			else
			{
				base.OnTransitionOutStarted(handle);
			}
		}
		else if (ExitWrapper == null || handle.ActiveAnimation.GetActiveClip() == ExitWrapper.AnimationClip)
		{
			base.OnTransitionOutStarted(handle);
		}
	}

	public void SwitchToExit(UnitAnimationActionHandle handle)
	{
		HandleData handleData = handle.ActionData as HandleData;
		if (!handleData.IsExiting)
		{
			handleData.IsExiting = true;
			if ((bool)ExitWrapper)
			{
				handle.StartClip(ExitWrapper, ClipDurationType.Oneshot);
			}
			else
			{
				handle.Release();
			}
		}
	}
}
