using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("88af5a4cce274f2ea8c390778d80b756")]
public class AbilityCustomDirectedFirestorm : AbilityCustomLogic
{
	public bool DesperateMeasure;

	[SerializeField]
	private BlueprintBuffReference m_DesperateMeasureBuff;

	[SerializeField]
	private BlueprintBuffReference m_DesperateMeasureCasterBuff;

	[SerializeField]
	private BlueprintUnitFactReference m_Upgrade1Fact;

	[SerializeField]
	private BlueprintUnitFactReference m_Upgrade2Fact;

	[SerializeField]
	private BlueprintUnitFactReference m_Upgrade3Fact;

	[SerializeField]
	private BlueprintBuffReference m_Upgrade3Buff;

	[SerializeField]
	private BlueprintUnitFactReference m_Upgrade4Fact;

	[SerializeField]
	private BlueprintBuffReference m_Upgrade4Buff;

	public BlueprintBuff DesperateMeasureBuff => m_DesperateMeasureBuff.Get();

	public BlueprintBuff DesperateMeasureCasterBuff => m_DesperateMeasureCasterBuff.Get();

	public BlueprintUnitFact Upgrade1Fact => m_Upgrade1Fact.Get();

	public BlueprintUnitFact Upgrade2Fact => m_Upgrade2Fact.Get();

	public BlueprintUnitFact Upgrade3Fact => m_Upgrade3Fact.Get();

	public BlueprintBuff Upgrade3Buff => m_Upgrade3Buff.Get();

	public BlueprintUnitFact Upgrade4Fact => m_Upgrade4Fact.Get();

	public BlueprintBuff Upgrade4Buff => m_Upgrade4Buff.Get();

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		MechanicEntity caster = context.Caster;
		AbilitySelectTarget component = context.AbilityBlueprint.GetComponent<AbilitySelectTarget>();
		AbilityDeliveryTarget abilityDeliveryTarget = new AbilityDeliveryTarget(context.ClickedTarget);
		if (component == null)
		{
			yield break;
		}
		IEnumerable<TargetWrapper> enumerable = component.Select(context, abilityDeliveryTarget.Target);
		List<UnitEntity> enemies = new List<UnitEntity>();
		BuffDuration duration = new BuffDuration(1.Rounds(), BuffEndCondition.CombatEnd);
		BuffDuration duration2 = new BuffDuration(null, BuffEndCondition.CombatEnd);
		foreach (TargetWrapper item in enumerable)
		{
			UnitEntity unitEntity = item.Entity as UnitEntity;
			if (unitEntity != null && unitEntity.IsAlly(caster) && caster.Facts.Contains(Upgrade4Fact))
			{
				unitEntity.Buffs.Add(Upgrade4Buff, context, duration);
			}
			if (unitEntity != null && !unitEntity.Features.IsUntargetable && unitEntity.IsEnemy(caster) && !unitEntity.LifeState.IsDead)
			{
				enemies.Add(unitEntity);
			}
		}
		IEnumerable<AbstractUnitEntity> enumerable2 = Game.Instance.State.AllUnits.Where((AbstractUnitEntity p1) => !p1.Features.IsUntargetable && !p1.LifeState.IsDead && p1.IsInCombat && p1.IsAlly(caster));
		foreach (AbstractUnitEntity item2 in enumerable2)
		{
			if (caster.Facts.Contains(Upgrade3Fact))
			{
				item2.Buffs.Add(Upgrade3Buff, context, duration);
			}
		}
		WarhammerContextActionAttackNearestTarget.PriorityTargetAttackSelectType attackType = ((caster.Facts.Contains(Upgrade3Fact) && caster.Facts.Contains(Upgrade4Fact)) ? WarhammerContextActionAttackNearestTarget.PriorityTargetAttackSelectType.LowestCostButBurstOrAoE : (caster.Facts.Contains(Upgrade4Fact) ? WarhammerContextActionAttackNearestTarget.PriorityTargetAttackSelectType.LowestCostButAoE : ((!caster.Facts.Contains(Upgrade3Fact)) ? WarhammerContextActionAttackNearestTarget.PriorityTargetAttackSelectType.LowestCost : WarhammerContextActionAttackNearestTarget.PriorityTargetAttackSelectType.LowestCostButBurst)));
		List<AbstractUnitEntity> list = enumerable2.Where((AbstractUnitEntity p1) => enemies.Any((UnitEntity p2) => LosCalculations.GetWarhammerLos(p1, p2).CoverType != LosCalculations.CoverType.Invisible && WarhammerContextActionAttackNearestTarget.SelectAttackAbility(p1, p2, attackType, new List<BlueprintAbilityGroupReference>()) != null)).ToList();
		if (caster.Facts.Contains(Upgrade1Fact))
		{
			list.Add(list.Random(PFStatefulRandom.Mechanics));
			if (list.Contains(caster))
			{
				list.Add((UnitEntity)caster);
			}
		}
		foreach (AbstractUnitEntity ally in list)
		{
			UnitEntity unitEntity2 = enemies.Where((UnitEntity p) => LosCalculations.GetWarhammerLos(ally, p).CoverType != LosCalculations.CoverType.Invisible && WarhammerContextActionAttackNearestTarget.SelectAttackAbility(ally, p, attackType, new List<BlueprintAbilityGroupReference>()) != null).ToList().Random(PFStatefulRandom.Mechanics);
			Ability ability = WarhammerContextActionAttackNearestTarget.SelectAttackAbility(ally, unitEntity2, attackType, new List<BlueprintAbilityGroupReference>());
			if (ability != null)
			{
				PartUnitCommands commandsOptional = ally.GetCommandsOptional();
				if (commandsOptional != null)
				{
					UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability.Data, unitEntity2)
					{
						IgnoreCooldown = true,
						FreeAction = true
					};
					commandsOptional.AddToQueue(cmdParams);
				}
				else
				{
					Rulebook.Trigger(new RulePerformAbility(ability.Data, unitEntity2)
					{
						IgnoreCooldown = true,
						ForceFreeAction = true
					});
				}
				if (DesperateMeasure)
				{
					ally.Buffs.Add(DesperateMeasureBuff, context, duration2);
				}
			}
		}
		if (DesperateMeasure)
		{
			caster.Buffs.Add(DesperateMeasureCasterBuff, context, duration2);
		}
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}
}
