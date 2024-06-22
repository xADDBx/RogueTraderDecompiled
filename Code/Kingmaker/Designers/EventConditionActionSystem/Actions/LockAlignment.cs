using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("beb863f2cd06fbf4f924982b22b7af89")]
public class LockAlignment : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[AlignmentMask]
	[InfoBox("Current unit alignment will be shifted to the nearest available sector.\nNone selected => removes locking and current alignment remains.")]
	public AlignmentMaskType AlignmentMask;

	[InfoBox("Considered as initial alignment. This is usually sector's center (LG for Angel, NE for Lich)\nWill be ignored for `None` mask (all or none square selected)")]
	public Alignment TargetAlignment;

	public override string GetDescription()
	{
		return $"Locks alignment mask for: {Unit}";
	}

	public override string GetCaption()
	{
		return $"Locks alignment mask for: {Unit}";
	}

	protected override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			baseUnitEntity.Alignment.LockAlignment(AlignmentMask, TargetAlignment);
		}
	}
}
