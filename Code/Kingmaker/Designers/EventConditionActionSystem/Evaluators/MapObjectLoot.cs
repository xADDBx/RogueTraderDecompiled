using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[PlayerUpgraderAllowed(false)]
[TypeId("ea4a209594d39fb47af898892a127a0e")]
public class MapObjectLoot : ItemsCollectionEvaluator
{
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public override string GetDescription()
	{
		return $"Возвращает лут из мапобжекта {MapObject}";
	}

	protected override ItemsCollection GetValueInternal()
	{
		return MapObject.GetValue().GetOptional<InteractionLootPart>()?.Loot;
	}

	public override string GetCaption()
	{
		return $"MapObject {MapObject} Loot";
	}
}
