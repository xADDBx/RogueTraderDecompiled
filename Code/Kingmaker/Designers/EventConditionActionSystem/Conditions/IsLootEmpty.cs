using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(true)]
[TypeId("be179216bf174b808288ad7b60b70959")]
public class IsLootEmpty : Condition
{
	[HideIf("EvaluateMapObject")]
	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference LootObject;

	[ShowIf("EvaluateMapObject")]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public bool EvaluateMapObject;

	protected override string GetConditionCaption()
	{
		return "Is loot " + ((!EvaluateMapObject) ? LootObject.ToString() : MapObject?.ToString()) + " empty?";
	}

	protected override bool CheckCondition()
	{
		if (EvaluateMapObject)
		{
			return MapObject.GetValue().GetOptional<InteractionLootPart>()?.Loot.Empty() ?? false;
		}
		IEntity entity = LootObject.FindData();
		if (entity == null)
		{
			return false;
		}
		return ((Entity)entity).GetOptional<InteractionLootPart>()?.Loot.Empty() ?? false;
	}
}
