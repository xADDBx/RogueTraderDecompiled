using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintAnswer))]
[AllowMultipleComponents]
[TypeId("b630897d1c2f4f41ab197acbe74d14b7")]
public class RequirementResourceUseDialog : Requirement
{
	[SerializeField]
	private ResourceData Resource;

	[SerializeField]
	public bool UseProfitFactorInstead;

	public BlueprintResource ResourceBlueprint => Resource?.Resource?.Get();

	public int Count => Resource?.Count ?? 0;

	public override bool Check(Colony colony = null)
	{
		if (ResourceBlueprint == null)
		{
			PFLog.System.Error("RequirementResourceUseOrder - resources is null or empty!");
			return true;
		}
		if (UseProfitFactorInstead)
		{
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
	}
}
