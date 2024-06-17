using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.UI.Models.Log.ContextFlag;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("d49f038b2ebaba34994fe5a797ecaeef")]
public class UnequipItem : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[InfoBox(Text = "If not specified, item will be moved to unit's inventory")]
	[SerializeReference]
	public ItemsCollectionEvaluator DestinationContainer;

	public bool Silent;

	[SerializeField]
	[FormerlySerializedAs("Item")]
	private BlueprintItemReference m_Item;

	public bool All = true;

	public BlueprintItem Item => m_Item?.Get();

	public override string GetDescription()
	{
		return $"Снимает предмет {Item?.Name} с юнита {Unit}";
	}

	public override string GetCaption()
	{
		return $"Unequip {Item} from {Unit}";
	}

	public override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
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
			foreach (ItemSlot equipmentSlot in baseUnitEntity.Body.EquipmentSlots)
			{
				if (equipmentSlot.MaybeItem?.Blueprint == Item)
				{
					if (value != null)
					{
						ItemEntity item = equipmentSlot.Item;
						item.Collection.Remove(item);
						value.Add(item);
					}
					else
					{
						equipmentSlot.RemoveItem();
					}
					if (!All)
					{
						break;
					}
				}
			}
		}
	}
}
