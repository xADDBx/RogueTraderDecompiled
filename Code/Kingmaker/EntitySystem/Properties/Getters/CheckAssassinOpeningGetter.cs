using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("faf78136b4a0688418f1964347209313")]
public class CheckAssassinOpeningGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override string GetInnerCaption()
	{
		return "Hit an Assassin Opening";
	}

	protected override int GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is UnitEntity unitEntity))
		{
			return 0;
		}
		UnitPartSideVulnerability optional = unitEntity.Parts.GetOptional<UnitPartSideVulnerability>();
		if (optional == null)
		{
			return 0;
		}
		bool flag = false;
		if (unitEntity.Facts.Get((BlueprintBuff)Root.WH.CombatRoot.AssassinKeystoneBuff) != null)
		{
			UnitPartSideVulnerability.Entry entry = optional.Get(unitEntity.Facts.Get((BlueprintBuff)Root.WH.CombatRoot.AssassinKeystoneBuff));
			if (entry != null)
			{
				Vector3 normalized = (unitEntity.Center - base.CurrentEntity.Center).normalized;
				WarhammerCombatSide warhammerAttackSide = CustomGraphHelper.GetWarhammerAttackSide(unitEntity.Forward, normalized, unitEntity.Size);
				if (entry.UnitSide == warhammerAttackSide)
				{
					flag = true;
				}
			}
		}
		if (unitEntity.Facts.Get((BlueprintBuff)Root.WH.CombatRoot.AssassinKeystoneBuffOpening) != null)
		{
			UnitPartSideVulnerability.Entry entry2 = optional.Get(unitEntity.Facts.Get((BlueprintBuff)Root.WH.CombatRoot.AssassinKeystoneBuffOpening));
			if (entry2 != null)
			{
				Vector3 normalized2 = (unitEntity.Center - base.CurrentEntity.Center).normalized;
				WarhammerCombatSide warhammerAttackSide2 = CustomGraphHelper.GetWarhammerAttackSide(unitEntity.Forward, normalized2, unitEntity.Size);
				if (entry2.UnitSide == warhammerAttackSide2)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return 0;
		}
		return 1;
	}
}
