using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker;

[TypeId("38012c7653cb90048918805178d81622")]
public class AbilityPassedThroughAreaEffectGetter : MechanicEntityPropertyGetter
{
	private enum AreaCasterType
	{
		Any,
		Ally,
		Enemy
	}

	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_AreaEffect;

	[SerializeField]
	private AreaCasterType m_AreaCasterType;

	private MechanicsContext CurrentContext => ContextData<MechanicsContext.Data>.Current?.Context;

	protected override int GetBaseValue()
	{
		if (CurrentContext == null)
		{
			return 0;
		}
		if (CurrentContext.PassedAreaEffects == null)
		{
			RuleCalculatePassedAreaEffects ruleCalculatePassedAreaEffects = new RuleCalculatePassedAreaEffects(CurrentContext.MaybeCaster, (CustomGridNodeBase)(CurrentContext.MaybeCaster?.CurrentNode.node), CurrentContext.MainTarget.NearestNode);
			Rulebook.Trigger(ruleCalculatePassedAreaEffects);
			CurrentContext.PassedAreaEffects = ruleCalculatePassedAreaEffects.PassedAreas;
		}
		BlueprintAbilityAreaEffect blueprintAbilityAreaEffect = m_AreaEffect.Get();
		foreach (EntityRef<AreaEffectEntity> passedAreaEffect in CurrentContext.PassedAreaEffects)
		{
			AreaEffectEntity entity = passedAreaEffect.Entity;
			if (entity.Blueprint == blueprintAbilityAreaEffect && IsSuitableCaster(entity.Context.MaybeCaster))
			{
				return 1;
			}
		}
		return 0;
	}

	private bool IsSuitableCaster(MechanicEntity caster)
	{
		return m_AreaCasterType switch
		{
			AreaCasterType.Any => true, 
			AreaCasterType.Ally => caster?.IsAlly(caster) ?? false, 
			AreaCasterType.Enemy => caster?.IsEnemy(caster) ?? false, 
			_ => false, 
		};
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Has the attack passed through " + m_AreaEffect.NameSafe() + " area effect.";
	}
}
