using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowMultipleComponents]
[TypeId("e358bb9eda304804bc36c2b8b0e704e5")]
public class RequirementResourceUseProject : Requirement
{
	[SerializeField]
	private ResourceData Resource;

	public BlueprintResource ResourceBlueprint => Resource?.Resource?.Get();

	public int Count => Resource?.Count ?? 0;

	public override bool Check(Colony colony = null)
	{
		if (ResourceBlueprint == null)
		{
			PFLog.System.Error("RequirementResourceUseProject - resource is null!");
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
		if (colony != null && ResourceBlueprint != null)
		{
			BlueprintColonyProject projectBp = base.OwnerBlueprint as BlueprintColonyProject;
			colony.SpendResource(projectBp, Resource);
		}
	}
}
