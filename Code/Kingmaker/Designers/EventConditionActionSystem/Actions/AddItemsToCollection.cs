using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("6fa8c37d53264cfc9f73ce7f3004f492")]
public class AddItemsToCollection : GameAction
{
	[SerializeReference]
	public ItemsCollectionEvaluator ItemsCollection;

	public bool UseBlueprintUnitLoot;

	[HideIf("UseBlueprintUnitLoot")]
	public List<LootEntry> Loot;

	[ShowIf("UseBlueprintUnitLoot")]
	[SerializeField]
	[FormerlySerializedAs("BlueprintLoot")]
	private BlueprintUnitLootReference m_BlueprintLoot;

	public bool Silent;

	public BlueprintUnitLoot BlueprintLoot => m_BlueprintLoot?.Get();

	public override string GetDescription()
	{
		return $"Добавляет передметы в коллекцию итемов {ItemsCollection}\n" + $"Брать предметы из настроек BlueprintUnitLoot: {UseBlueprintUnitLoot}";
	}

	public override string GetCaption()
	{
		return $"Add items to collection ({ItemsCollection})";
	}

	protected override void RunAction()
	{
		using (ContextData<GameLogDisabled>.RequestIf(Silent))
		{
			ItemsCollection inventory = ItemsCollection.GetValue();
			if (UseBlueprintUnitLoot)
			{
				foreach (LootEntry item in BlueprintLoot.GenerateItems())
				{
					inventory.Add(item.Item, item.Count);
				}
				return;
			}
			Loot.ForEach(delegate(LootEntry i)
			{
				inventory.Add(i.Item, i.Count);
			});
		}
	}
}
