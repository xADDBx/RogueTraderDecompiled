using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("57055a7958e0457cb16c1654f5bb4826")]
public class RemoveItemsFromCollection : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public ItemsCollectionEvaluator Collection;

	public List<LootEntry> Loot;

	public override string GetDescription()
	{
		return $"Удаляет указанные предметы из коллекции {Collection}.";
	}

	public override string GetCaption()
	{
		return $"Remove items from ({Collection})";
	}

	protected override void RunAction()
	{
		ItemsCollection collection = Collection.GetValue();
		Loot.ForEach(delegate(LootEntry i)
		{
			collection.Remove(i.Item, i.Count);
		});
	}
}
