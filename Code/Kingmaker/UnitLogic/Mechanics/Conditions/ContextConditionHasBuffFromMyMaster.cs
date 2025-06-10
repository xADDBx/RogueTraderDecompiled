using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("b46e6e49ab07447189426fb639c907a9")]
public class ContextConditionHasBuffFromMyMaster : ContextCondition
{
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override string GetConditionCaption()
	{
		return "Check if target has " + Buff.Name + " from master of caster";
	}

	protected override bool CheckCondition()
	{
		BaseUnitEntity baseUnitEntity = (base.Context.MaybeCaster as BaseUnitEntity)?.Master;
		if (baseUnitEntity == null)
		{
			PFLog.Actions.Error("ContextConditionHasBuffFromMyPet called from caster without a master");
			return false;
		}
		foreach (Buff buff in base.Target.Entity.Buffs)
		{
			if (buff.Blueprint == Buff && buff.Context.MaybeCaster == baseUnitEntity)
			{
				return true;
			}
		}
		return false;
	}
}
