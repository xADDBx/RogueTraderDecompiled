using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.UI.Models.Log.ContextFlag;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("279d4ee480d04f2b99e22da75adf77d6")]
public class UnequipAllItems : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[InfoBox(Text = "If not specified, items will be moved to unit's inventory")]
	[SerializeReference]
	public ItemsCollectionEvaluator DestinationContainer;

	public bool Silent;

	public override string GetCaption()
	{
		return $"Unequip all items from ({Target}) inventory";
	}

	public override void RunAction()
	{
		if (!(Target.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Target} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		ItemsCollection value = null;
		DestinationContainer?.TryGetValue(out value);
		using (ContextData<GameLogDisabled>.RequestIf(Silent))
		{
			foreach (ItemSlot allSlot in baseUnitEntity.Body.AllSlots)
			{
				if (allSlot.HasItem && allSlot.CanRemoveItem())
				{
					if (value != null)
					{
						ItemEntity item = allSlot.Item;
						item.Collection.Remove(item);
						value.Add(item);
					}
					else
					{
						allSlot.RemoveItem();
					}
				}
			}
		}
	}
}
