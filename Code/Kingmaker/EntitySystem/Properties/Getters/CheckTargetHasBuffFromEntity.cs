using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("82e9cd41675f4262b09a9ae2e42a537a")]
public class CheckTargetHasBuffFromEntity : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	[SerializeField]
	public PropertyTargetType CheckTargetHasBuffFrom;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override int GetBaseValue()
	{
		MechanicEntity currentTarget = base.PropertyContext.CurrentTarget;
		MechanicEntity targetEntity = base.PropertyContext.GetTargetEntity(CheckTargetHasBuffFrom);
		if (currentTarget == null || targetEntity == null)
		{
			return 0;
		}
		foreach (Buff buff in currentTarget.Buffs)
		{
			if (buff.Blueprint == Buff && buff.Context.MaybeCaster == targetEntity)
			{
				return 1;
			}
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return string.Concat("Check if target has buff from entity");
	}
}
