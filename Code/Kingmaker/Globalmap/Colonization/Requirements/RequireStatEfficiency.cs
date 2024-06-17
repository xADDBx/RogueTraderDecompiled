using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("5fafbc860bd3431d908d2da53bd85c51")]
public class RequireStatEfficiency : Requirement
{
	[SerializeField]
	private int m_MinEfficiencyValue;

	public int MinEfficiencyValue => m_MinEfficiencyValue;

	public override bool Check(Colony colony = null)
	{
		if (colony != null)
		{
			return colony.Efficiency.Value >= m_MinEfficiencyValue;
		}
		return false;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
