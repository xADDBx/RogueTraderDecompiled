using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("d049148cefe24aeebe1fc1c10414f7db")]
public class RequirementReputationCost : Requirement
{
	[SerializeField]
	public int Reputation;

	[SerializeField]
	public FactionType Faction;

	public override bool Check(Colony colony = null)
	{
		return ReputationHelper.ReputationPointsReached(Faction, Reputation);
	}

	public override void Apply(Colony colony = null)
	{
		ReputationHelper.GainFactionReputation(Faction, -Reputation);
	}
}
