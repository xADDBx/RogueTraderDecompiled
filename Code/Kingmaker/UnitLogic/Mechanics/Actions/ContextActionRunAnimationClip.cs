using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("9607136c02686cd448729766a779cb1d")]
public class ContextActionRunAnimationClip : ContextAction
{
	[Obsolete]
	[HideInInspector]
	public AnimationClipWrapper ClipWrapper;

	public AnimationClipWrapperLink ClipWrapperLink;

	public float TransitionIn = 0.25f;

	public float TransitionOut = 0.25f;

	public ExecutionMode Mode;

	public override string GetCaption()
	{
		return "Run animation clip on target";
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity != null && entity.MaybeAnimationManager != null)
		{
			UnitAnimationActionClip unitAnimationActionClip = UnitAnimationActionClip.Create(ClipWrapperLink.Load(), "RunAction");
			unitAnimationActionClip.TransitionIn = TransitionIn;
			unitAnimationActionClip.TransitionOut = TransitionOut;
			unitAnimationActionClip.ExecutionMode = Mode;
			AnimationActionHandle handle = entity.MaybeAnimationManager.CreateHandle(unitAnimationActionClip);
			entity.MaybeAnimationManager.Execute(handle);
		}
	}
}
