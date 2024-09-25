using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[TypeId("65671d15080b4532a019b3576b24d6c1")]
public class RequirementNotBuiltProjectInColony : Requirement
{
	[SerializeField]
	private BlueprintColonyProjectReference m_NotBuiltProject;

	public BlueprintColonyProject NotBuiltProject => m_NotBuiltProject?.Get();

	public override bool Check(Colony colony = null)
	{
		if (NotBuiltProject != null)
		{
			if (colony != null)
			{
				return colony.Projects.FirstOrDefault((ColonyProject project) => project.Blueprint == NotBuiltProject) == null;
			}
			return false;
		}
		return true;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
