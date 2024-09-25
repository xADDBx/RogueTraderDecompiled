using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.Items;

[TypeId("4ef69d4a02ba4951b16d80e1b6a9bb04")]
public class ItemFromUnitEvaluator : ItemEvaluator
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_Blueprint;

	[CanBeNull]
	[SerializeReference]
	public AbstractUnitEvaluator Wielder;

	public BlueprintItem Blueprint => m_Blueprint;

	public override string GetCaption()
	{
		if (Wielder != null)
		{
			return $"Item [{Blueprint}] equipped by [{Wielder}]";
		}
		return $"Item [{Blueprint}] from player's inventory";
	}

	protected override ItemEntity GetValueInternal()
	{
		if (Wielder != null)
		{
			if (!(Wielder.GetValue() is BaseUnitEntity baseUnitEntity))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Evaluator {this}, {Wielder} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return null;
			}
			foreach (ItemSlot equipmentSlot in baseUnitEntity.Body.EquipmentSlots)
			{
				if (equipmentSlot.MaybeItem?.Blueprint == Blueprint)
				{
					return equipmentSlot.MaybeItem;
				}
			}
		}
		foreach (ItemEntity item in Game.Instance.Player.Inventory)
		{
			if (item.Blueprint == Blueprint)
			{
				return item;
			}
		}
		throw new FailToEvaluateException(this);
	}
}
