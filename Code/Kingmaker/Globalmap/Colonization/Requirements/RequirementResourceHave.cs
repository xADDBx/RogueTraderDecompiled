using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("185bebc1634848bc9dc78c6268e20ff3")]
public class RequirementResourceHave : Requirement
{
	[SerializeField]
	private ResourceData m_Resource;

	public BlueprintResource Resource => m_Resource?.Resource?.Get();

	public int Count => m_Resource.Count;

	public override bool Check(Colony colony = null)
	{
		if (Resource == null)
		{
			PFLog.System.Error("RequirementResourceHave - resource is null!");
			return true;
		}
		if (Game.Instance.ColonizationController.AllResourcesInPool().TryGetValue(Resource, out var value))
		{
			return value >= Count;
		}
		return false;
	}

	public override void Apply(Colony colony)
	{
	}
}
