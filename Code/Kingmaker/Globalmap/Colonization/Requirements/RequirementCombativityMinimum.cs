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
[TypeId("6980cc2439d7b384cb68f3968c549703")]
public class RequirementCombativityMinimum : Requirement
{
	[SerializeField]
	private int m_CombativityMinimum;

	public int CombativityMinimum => m_CombativityMinimum;

	public override bool Check(Colony colony = null)
	{
		return Game.Instance.Player.Combativity.Total >= (float)m_CombativityMinimum;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
