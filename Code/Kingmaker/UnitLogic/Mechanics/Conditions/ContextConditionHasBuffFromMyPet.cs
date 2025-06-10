using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("23cc959b0b094f95881996b0ff1c77ae")]
public class ContextConditionHasBuffFromMyPet : ContextCondition
{
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override string GetConditionCaption()
	{
		return "Check if target has " + Buff.Name + " from pet of caster";
	}

	protected override bool CheckCondition()
	{
		BaseUnitEntity baseUnitEntity = base.Context.MaybeCaster?.GetOptional<UnitPartPetOwner>()?.PetUnit;
		if (baseUnitEntity == null)
		{
			PFLog.Actions.Error("ContextConditionHasBuffFromMyPet called from caster without a pet");
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
