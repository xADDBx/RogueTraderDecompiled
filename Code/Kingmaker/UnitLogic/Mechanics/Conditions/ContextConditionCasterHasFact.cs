using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("43f75bd73450e1f4aa1f6163f51956e3")]
public class ContextConditionCasterHasFact : ContextCondition
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
		if (base.Context?.MaybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return false;
		}
		return base.Context.MaybeCaster.Facts.Contains(Fact);
	}
}
