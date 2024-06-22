using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.ContextFlag;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("e7e441541aea4730831afbd223348bf5")]
public class StashItemFromUnitInventorySlot : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator TargetUnit;

	public EquipSlotType TargetSlot;

	[SerializeReference]
	public ItemsCollectionEvaluator TargetStash;

	public bool Silent;

	public override string GetCaption()
	{
		return $"Снимает у юнита {TargetUnit?.name} предмет из слота {TargetSlot} и помещает в стеш {TargetStash?.name}";
	}

	protected override void RunAction()
	{
		if (!(TargetUnit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {TargetUnit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		ItemsCollection value = null;
		TargetStash?.TryGetValue(out value);
		if (value == null)
		{
			return;
		}
		using (ContextData<GameLogDisabled>.RequestIf(Silent))
		{
			ItemSlot equipSlot = baseUnitEntity.Body.GetEquipSlot(TargetSlot, 0);
			if (equipSlot.HasItem && equipSlot.CanRemoveItem())
			{
				ItemEntity item = equipSlot.Item;
				item.Collection.Transfer(item, value);
			}
		}
	}
}
