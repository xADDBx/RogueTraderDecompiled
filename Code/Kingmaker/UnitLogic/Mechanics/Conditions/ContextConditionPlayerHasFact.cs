using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("159b144595c041a688a14f73d2624250")]
public class ContextConditionPlayerHasFact : ContextCondition
{
	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		return GameHelper.GetPlayerCharacter().Facts.Contains(Fact);
	}
}
