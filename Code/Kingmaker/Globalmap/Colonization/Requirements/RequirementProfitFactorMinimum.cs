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
[TypeId("6ffdadf391194bd1960f6bd0ee30a785")]
public class RequirementProfitFactorMinimum : Requirement
{
	[SerializeField]
	private int m_ProfitFactorMinimum;

	public int ProfitFactorMinimum => m_ProfitFactorMinimum;

	public override bool Check(Colony colony = null)
	{
		return Game.Instance.Player.ProfitFactor.Total >= (float)m_ProfitFactorMinimum;
	}

	public override void Apply(Colony colony = null)
	{
	}
}
