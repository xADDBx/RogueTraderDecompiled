using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("d56ec97f49674c92b5d87e101eda2c9e")]
public class PlayAnimationOneShot : GameAction
{
	[SerializeField]
	[Obsolete]
	[HideInInspector]
	private AnimationClipWrapper m_ClipWrapper;

	[SerializeField]
	private AnimationClipWrapperLink m_ClipWrapperLink;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public float TransitionIn = 0.25f;

	public float TransitionOut = 0.25f;

	public bool IsClipWrapperSet => m_ClipWrapperLink?.Exists() ?? false;

	protected override void RunAction()
	{
		AbstractUnitEntity value = Unit.GetValue();
		if (value.View.AnimationManager != null)
		{
			if (!IsClipWrapperSet)
			{
				throw new Exception("Animation clip wrapper is not set.");
			}
			UnitAnimationActionClip unitAnimationActionClip = UnitAnimationActionClip.Create(m_ClipWrapperLink.Load(), "RunAction");
			unitAnimationActionClip.TransitionIn = TransitionIn;
			unitAnimationActionClip.TransitionOut = TransitionOut;
			unitAnimationActionClip.ExecutionMode = ExecutionMode.Interrupted;
			AnimationActionHandle handle = value.View.AnimationManager.CreateHandle(unitAnimationActionClip);
			value.View.AnimationManager.Execute(handle);
		}
	}

	public override string GetCaption()
	{
		string assetId = m_ClipWrapperLink.AssetId;
		return $"{Unit} play one-shot {assetId}";
	}
}
