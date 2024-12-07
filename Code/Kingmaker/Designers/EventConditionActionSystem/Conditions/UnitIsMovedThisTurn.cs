using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitIsMovedThisTurn")]
[AllowMultipleComponents]
[TypeId("5388d48db2277394cad64d7626c10400")]
public class UnitIsMovedThisTurn : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	private bool CheckMovedMoreCellsThanAmount;

	[ShowIf("CheckMovedMoreCellsThanAmount")]
	[SerializeField]
	private float CellsAmount;

	protected override string GetConditionCaption()
	{
		return $"({Unit}) moved this turn";
	}

	protected override bool CheckCondition()
	{
		PartUnitCombatState combatStateOptional = Unit.GetValue().GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			return false;
		}
		if (!CheckMovedMoreCellsThanAmount)
		{
			return combatStateOptional.IsMovedThisTurn;
		}
		if (combatStateOptional.IsMovedThisTurn)
		{
			return combatStateOptional.MovedCellsThisTurn > CellsAmount;
		}
		return false;
	}
}
