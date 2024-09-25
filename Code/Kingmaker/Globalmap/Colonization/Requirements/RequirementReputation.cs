using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Controllers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[AllowMultipleComponents]
[TypeId("103ff90d034f4fa29f5250f54ba296ed")]
public class RequirementReputation : Requirement
{
	[SerializeField]
	private FactionReputation m_Reputation;

	public FactionReputation Reputation => m_Reputation;

	public override bool Check(Colony colony = null)
	{
		return ReputationHelper.GetCurrentReputationLevel(m_Reputation.Faction) >= m_Reputation.MinLevelValue;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
