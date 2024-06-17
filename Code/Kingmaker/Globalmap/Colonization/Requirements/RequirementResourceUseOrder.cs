using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintQuestContract))]
[TypeId("feb2e0d661b64c6d8d255a0b8c3fb239")]
[AllowMultipleComponents]
public class RequirementResourceUseOrder : Requirement
{
	[SerializeField]
	private ResourceData Resource;

	public BlueprintResource ResourceBlueprint => Resource?.Resource?.Get();

	public int Count => Resource?.Count ?? 0;

	public override bool Check(Colony colony = null)
	{
		if (ResourceBlueprint == null)
		{
			PFLog.System.Error("RequirementResourceUseOrder - resources is null or empty!");
			return true;
		}
		if (Game.Instance.ColonizationController.AllResourcesInPool().TryGetValue(ResourceBlueprint, out var value))
		{
			return value >= Count;
		}
		return false;
	}

	public override void Apply(Colony colony = null)
	{
		Game.Instance.ColonizationController.UseResourceFromPool(ResourceBlueprint, Count);
		if (!Game.Instance.Player.ColoniesState.OrdersUseResources.Contains(base.OwnerBlueprint as BlueprintQuestContract))
		{
			Game.Instance.Player.ColoniesState.OrdersUseResources.Add(base.OwnerBlueprint as BlueprintQuestContract);
		}
	}
}
