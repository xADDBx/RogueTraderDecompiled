using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("7acf9343149568e42a5ccb414fc8806a")]
[PlayerUpgraderAllowed(true)]
public class ReequipMechadendriteOnUnit : GameAction
{
	[SerializeReference]
	[ValidateNotNull]
	public AbstractUnitEvaluator Target;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemMechadendrite.BlueprintItemMechadendriteReference m_RemoveMechadendrite;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemMechadendrite.BlueprintItemMechadendriteReference m_ReplaceMechadendrite;

	[NotNull]
	public BlueprintItem RemoveMechadendrite => (BlueprintItemMechadendrite)m_RemoveMechadendrite;

	[NotNull]
	public BlueprintItem ReplaceMechadendrite => (BlueprintItemMechadendrite)m_ReplaceMechadendrite;

	public override string GetDescription()
	{
		return "Снимает указанный мехадендрит с юнита и заменяет его новым.\n";
	}

	public override string GetCaption()
	{
		return $"Replace item {RemoveMechadendrite} with {ReplaceMechadendrite}";
	}

	protected override void RunAction()
	{
		if (!(Target.GetValue() is UnitEntity unitEntity))
		{
			return;
		}
		foreach (ItemSlot equipmentSlot in unitEntity.Body.EquipmentSlots)
		{
			if (equipmentSlot.MaybeItem?.Blueprint != RemoveMechadendrite)
			{
				continue;
			}
			equipmentSlot.RemoveItem(autoMerge: true, force: true);
			ItemEntity item = ReplaceMechadendrite.CreateEntity();
			if (equipmentSlot.IsItemSupported(item))
			{
				using (ContextData<ItemSlot.IgnoreLock>.Request())
				{
					equipmentSlot.InsertItem(item, force: true);
				}
			}
		}
	}
}
