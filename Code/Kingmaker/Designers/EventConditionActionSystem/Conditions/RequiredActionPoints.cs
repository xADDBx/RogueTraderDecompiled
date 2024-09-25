using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("2a359585b44d55d44889e16d8385e90d")]
public class RequiredActionPoints : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	private int RequiredAP;

	[SerializeField]
	private int RequiredMP;

	protected override string GetConditionCaption()
	{
		return $"{Unit} has AP >= {RequiredAP} and MP >= {RequiredMP}";
	}

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
		if (!baseUnitEntity.CombatState.IsInCombat)
		{
			return true;
		}
		if (baseUnitEntity.CombatState.ActionPointsYellow >= RequiredAP)
		{
			return baseUnitEntity.CombatState.ActionPointsBlue >= (float)RequiredMP;
		}
		return false;
	}
}
