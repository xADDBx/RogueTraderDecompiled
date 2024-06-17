using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("47b854d1c8dc4a22b15dea737ed17c4a")]
public class RequireStatSecurity : Requirement
{
	[SerializeField]
	private int m_MinSecurityValue;

	public int MinSecurityValue => m_MinSecurityValue;

	public override bool Check(Colony colony = null)
	{
		if (colony != null)
		{
			return colony.Security.Value >= m_MinSecurityValue;
		}
		return false;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
