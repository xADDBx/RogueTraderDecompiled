using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("0ecaa3519b1847d5baff0a5afc03d11a")]
public class UnitInActiveJump : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override bool CheckCondition()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Condition {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return false;
		}
		UnitPartJump optional = baseUnitEntity.Parts.GetOptional<UnitPartJump>();
		if (optional == null)
		{
			return false;
		}
		return optional.Active.JumpPhase != UnitPartJump.JumpPhaseType.Out;
	}

	protected override string GetConditionCaption()
	{
		return "Выдает true если Unit находится в активной стадии джампа";
	}
}
