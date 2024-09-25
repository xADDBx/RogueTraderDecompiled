using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("72a5648a7abcbd748b5d2a601acb616d")]
public class ContextConditionHasBuff : ContextCondition
{
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override string GetConditionCaption()
	{
		return "Check if target has " + Buff.Name;
	}

	protected override bool CheckCondition()
	{
		return base.Target.Entity.Buffs.Contains(Buff);
	}
}
