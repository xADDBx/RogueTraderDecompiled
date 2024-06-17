using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("00d0b0c8771b486c90548c840664bf51")]
public class RequirementStatContentment : Requirement
{
	[SerializeField]
	private int m_MinContentmentValue;

	public int MinContentmentValue => m_MinContentmentValue;

	public override bool Check(Colony colony = null)
	{
		if (colony != null)
		{
			return colony.Contentment.Value >= m_MinContentmentValue;
		}
		return false;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
