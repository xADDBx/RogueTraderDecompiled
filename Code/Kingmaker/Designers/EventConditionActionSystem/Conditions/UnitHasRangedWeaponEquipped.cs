using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("9ddee6a6ebce4240ab83fb2a6a5536ce")]
public class UnitHasRangedWeaponEquipped : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool CheckMainSlotOnly;

	public override string GetDescription()
	{
		return "Выдает true, если в текущем сете у юнита есть рейнжовое оружие";
	}

	protected override string GetConditionCaption()
	{
		return $"Returns true if {Unit} has a ranged weapon equipped";
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
		if (CheckMainSlotOnly)
		{
			return baseUnitEntity.GetFirstWeapon()?.Blueprint.IsRanged ?? false;
		}
		foreach (HandsEquipmentSet handsEquipmentSet in baseUnitEntity.Body.HandsEquipmentSets)
		{
			if (!handsEquipmentSet.IsEmpty() && (handsEquipmentSet.PrimaryHand.MaybeItem?.Blueprint is BlueprintItemWeapon { IsRanged: not false } || handsEquipmentSet.SecondaryHand.MaybeItem?.Blueprint is BlueprintItemWeapon { IsRanged: not false }))
			{
				return true;
			}
		}
		return false;
	}
}
