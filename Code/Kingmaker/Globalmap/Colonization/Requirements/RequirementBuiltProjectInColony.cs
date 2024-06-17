using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[TypeId("4e862cff9aff4bc7a099ebfaad96b51f")]
public class RequirementBuiltProjectInColony : Requirement
{
	[SerializeField]
	private BlueprintColonyProjectReference m_BuiltProject;

	public BlueprintColonyProject BuiltProject => m_BuiltProject?.Get();

	public override bool Check(Colony colony = null)
	{
		if (BuiltProject != null)
		{
			return colony?.Projects.FirstOrDefault((ColonyProject project) => project.Blueprint == BuiltProject) != null;
		}
		return true;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
