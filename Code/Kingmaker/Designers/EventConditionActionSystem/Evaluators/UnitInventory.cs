using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("ac0b2a9e4daea5c42b85d77e48d5a304")]
public class UnitInventory : ItemsCollectionEvaluator
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetDescription()
	{
		return $"Возвращает инвентарь юнита {Unit}";
	}

	protected override ItemsCollection GetValueInternal()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Evaluator {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return null;
		}
		return baseUnitEntity.Inventory.Collection;
	}

	public override string GetCaption()
	{
		return $"{Unit} inventory";
	}
}
