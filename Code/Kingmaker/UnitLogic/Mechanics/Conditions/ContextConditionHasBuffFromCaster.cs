using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("2fc04e4a4ece686409d7b7857e09ed4c")]
public class ContextConditionHasBuffFromCaster : ContextCondition
{
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override string GetConditionCaption()
	{
		return "Check if target has " + Buff.Name + " from caster";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		foreach (Buff buff in base.Target.Entity.Buffs)
		{
			if (buff.Blueprint == Buff && buff.Context.MaybeCaster == maybeCaster)
			{
				return true;
			}
		}
		return false;
	}
}
