using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "AnimationCustomTongueGrab", menuName = "Animation Manager/Actions/Custom/Tongue Grab")]
public class UnitAnimationCustomTongueGrab : UnitAnimationAction
{
	public class HandleData
	{
		public bool EndAnimation;
	}

	[AssetPicker("")]
	[ValidateNotNull]
	public AnimationClipWrapper StartWrapper;

	[AssetPicker("")]
	[ValidateNotNull]
	public AnimationClipWrapper LoopWrapper;

	[AssetPicker("")]
	[ValidateNotNull]
	public AnimationClipWrapper EndWrapper;

	public override UnitAnimationType Type => UnitAnimationType.Unused;

	public override bool SupportCaching => true;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			yield return StartWrapper;
			yield return LoopWrapper;
			yield return EndWrapper;
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		HandleData actionData = new HandleData();
		handle.ActionData = actionData;
		handle.StartClip(StartWrapper, ClipDurationType.Oneshot);
		handle.ActEventsCounter++;
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		base.OnUpdate(handle, deltaTime);
		if (((HandleData)handle.ActionData).EndAnimation && handle.ActiveAnimation.GetActiveClip() == LoopWrapper.AnimationClip)
		{
			handle.StartClip(EndWrapper, ClipDurationType.Oneshot);
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (handle.ActiveAnimation?.GetActiveClip() == StartWrapper.AnimationClip)
		{
			handle.StartClip(LoopWrapper, ClipDurationType.Endless);
		}
		else if (handle.ActiveAnimation?.GetActiveClip() == EndWrapper.AnimationClip)
		{
			base.OnTransitionOutStarted(handle);
		}
	}
}
