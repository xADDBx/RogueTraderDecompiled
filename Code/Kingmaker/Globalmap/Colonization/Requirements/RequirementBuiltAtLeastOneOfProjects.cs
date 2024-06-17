using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[TypeId("c36dd42905d24eebb3332d890c9fdf0c")]
public class RequirementBuiltAtLeastOneOfProjects : Requirement
{
	[SerializeField]
	private List<BlueprintColonyProjectReference> m_Projects;

	public List<BlueprintColonyProject> Projects => (from projRef in m_Projects.EmptyIfNull()
		select projRef.Get()).ToList();

	public override bool Check(Colony colony = null)
	{
		if (m_Projects == null || m_Projects.Empty() || colony == null)
		{
			return true;
		}
		foreach (BlueprintColonyProject proj in Projects)
		{
			foreach (ColoniesState.ColonyData colony2 in Game.Instance.Player.ColoniesState.Colonies)
			{
				if (colony2.Colony.Projects.Any((ColonyProject project) => project.Blueprint == proj))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
