using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("bfe787e51845449dbd98425f134a555b")]
public class ContextActionAddBonusAbilityUsage : ContextAction
{
	[SerializeField]
	[CanBeNull]
	private RestrictionsHolder.Reference m_Restriction;

	[SerializeField]
	private PropertyCalculator m_Count;

	[SerializeField]
	private PropertyCalculator m_CostBonus;

	[SerializeField]
	private bool m_ToTarget;

	private static LogChannel Logger => BonusAbilityExtension.Logger;

	public override string GetCaption()
	{
		if (!m_ToTarget)
		{
			return "Add bonus ability usage to caster";
		}
		return "Add bonus ability usage to target";
	}

	public override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext == null)
		{
			Logger.Error(this, "Unable to add bonus ability usage: no context found");
			return;
		}
		MechanicEntity mechanicEntity = (m_ToTarget ? base.Target.Entity : mechanicsContext.MaybeCaster);
		if (mechanicEntity == null)
		{
			Logger.Error(this, "Unable to add bonus ability usage: target is null");
			return;
		}
		if (!(mechanicEntity is BaseUnitEntity baseUnitEntity))
		{
			Logger.Error(this, "Unable to add bonus ability usage: target is not BaseUnitEntity");
			return;
		}
		PropertyContext valueCalculationContext = GetValueCalculationContext(baseUnitEntity);
		EntityFactSource source = GetSource(baseUnitEntity);
		int value = m_Count.GetValue(valueCalculationContext);
		int value2 = m_CostBonus.GetValue(valueCalculationContext);
		baseUnitEntity.GetOrCreate<UnitPartBonusAbility>().AddBonusAbility(source, value, value2, m_Restriction);
	}

	private PropertyContext GetValueCalculationContext(BaseUnitEntity unit)
	{
		AreaEffectEntity areaEffectEntity = ContextData<AreaEffectContextData>.Current?.Entity;
		if (areaEffectEntity != null)
		{
			return new PropertyContext(areaEffectEntity, null).WithContext(base.Context);
		}
		if (ContextData<MechanicsContext.Data>.Current?.Context is AbilityExecutionContext abilityExecutionContext)
		{
			return new PropertyContext(abilityExecutionContext.Ability, unit).WithContext(base.Context);
		}
		return new PropertyContext(unit, null).WithContext(base.Context);
	}

	private EntityFactSource GetSource(BaseUnitEntity unit)
	{
		AreaEffectEntity areaEffectEntity = ContextData<AreaEffectContextData>.Current?.Entity;
		if (areaEffectEntity != null)
		{
			return new EntityFactSource(areaEffectEntity);
		}
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext is AbilityExecutionContext abilityExecutionContext)
		{
			return new EntityFactSource(abilityExecutionContext.AbilityBlueprint);
		}
		if (mechanicsContext != null)
		{
			return new EntityFactSource(mechanicsContext.AssociatedBlueprint);
		}
		return new EntityFactSource(unit);
	}
}
