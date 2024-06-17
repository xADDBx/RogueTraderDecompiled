using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.Items;

[TypeId("77a6f40bf3684c36ab07f85695c90a37")]
public class FirstItemFromLootEvaluator : ItemEvaluator
{
	[HideIf("EvaluateMapObject")]
	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference LootObject;

	[ShowIf("EvaluateMapObject")]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public bool EvaluateMapObject;

	public override string GetCaption()
	{
		return "First item from " + ((!EvaluateMapObject) ? LootObject.ToString() : MapObject?.ToString());
	}

	protected override ItemEntity GetValueInternal()
	{
		if (EvaluateMapObject)
		{
			return MapObject.GetValue().GetOptional<InteractionLootPart>()?.Loot.Items.FirstItem();
		}
		return LootObject.FindData()?.ToEntity().GetOptional<InteractionLootPart>()?.Loot.Items.FirstItem();
	}
}
