using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("9706de75454abeb48bd4cfa7f526a1c2")]
public class ContextConditionHasFact : ContextCondition
{
	[FormerlySerializedAs("Feature")]
	[SerializeField]
	[FormerlySerializedAs("Fact")]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		return base.Target.Entity.Facts.Contains(Fact);
	}
}
