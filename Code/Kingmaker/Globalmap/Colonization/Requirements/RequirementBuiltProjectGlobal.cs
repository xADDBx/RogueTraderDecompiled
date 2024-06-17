using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowMultipleComponents]
[TypeId("d0d6c340d59f4a6eaba89d28c0f31ea3")]
public class RequirementBuiltProjectGlobal : Requirement
{
	[SerializeField]
	private BlueprintColonyProjectReference m_BuiltProject;

	public BlueprintColonyProject BuiltProject => m_BuiltProject?.Get();

	public override bool Check(Colony colony = null)
	{
		foreach (ColoniesState.ColonyData colony2 in Game.Instance.Player.ColoniesState.Colonies)
		{
			if (colony2.Colony.Projects.FirstOrDefault((ColonyProject proj) => proj.Blueprint == BuiltProject && proj.IsFinished) != null)
			{
				return true;
			}
		}
		return false;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
