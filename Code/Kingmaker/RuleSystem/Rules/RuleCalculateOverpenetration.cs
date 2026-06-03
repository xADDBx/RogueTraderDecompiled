using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateOverpenetration : RulebookOptionalTargetEvent
{
	public readonly CompositeModifiersManager OverpenetrationFactorModifiers = new CompositeModifiersManager();

	public bool AllowRicochetWithoutHitting;

	private List<RicochetHelper.RicochetTargetData> m_RicochetTargets;

	public DamageData OverpenetrationDamage { get; private set; }

	public AbilityData Ability { get; private set; }

	private static int BaseOverpenetrationFactor => BlueprintWarhammerRoot.Instance.CombatRoot.BaseOverpenetrationChance;

	private static int BaseOverpenetrationReduction => BlueprintWarhammerRoot.Instance.CombatRoot.OverpenetrationReductionPerHit;

	public RuleCalculateOverpenetration([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] DamageData damageData, [NotNull] AbilityData ability)
		: base(initiator, target)
	{
		Ability = ability;
		OverpenetrationDamage = damageData?.Copy() ?? new DamageData(DamageType.None, 0);
		base.HasNoTarget = target == null;
		if (OverpenetrationDamage.Overpenetrating)
		{
			return;
		}
		int value = BaseOverpenetrationFactor;
		ItemEntityWeapon itemEntityWeapon = ability?.Weapon;
		if (itemEntityWeapon != null)
		{
			int? overrideOverpenetrationFactorPercents = itemEntityWeapon.Blueprint.OverrideOverpenetrationFactorPercents;
			int warhammerPenetration = itemEntityWeapon.Blueprint.WarhammerPenetration;
			if (overrideOverpenetrationFactorPercents.HasValue)
			{
				value = overrideOverpenetrationFactorPercents.Value;
			}
			OverpenetrationFactorModifiers.Add(ModifierType.ValAdd, warhammerPenetration, ModifierDescriptor.Weapon);
		}
		OverpenetrationFactorModifiers.Add(ModifierType.ValAdd, value, ModifierDescriptor.BaseValue);
	}

	public RuleCalculateOverpenetration([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, AbilityData ability)
		: this(initiator, target, null, ability)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		AbilityData ability = Ability;
		if ((object)ability != null && ability.IsMelee)
		{
			OverpenetrationDamage.IsRicochet = false;
			OverpenetrationDamage.IsRicochetFriendlyFireDisabled = false;
			OverpenetrationDamage.OverpenetrationFactorPercents = 0;
		}
		else
		{
			if ((MaybeTarget is DestructibleEntity && !OverpenetrationDamage.IsRicochet) || (MaybeTarget is UnitEntity entity && entity.HasMechanicFeature(MechanicsFeatureType.BlockOverpenetration)))
			{
				return;
			}
			int num = 0;
			if (MaybeTarget != null)
			{
				RuleCalculateStatsArmor ruleCalculateStatsArmor = new RuleCalculateStatsArmor(MaybeTarget);
				Rulebook.Trigger(ruleCalculateStatsArmor);
				num = ruleCalculateStatsArmor.ResultAbsorption;
			}
			SetUnreducedPenetration(base.InitiatorUnit, Ability);
			int overpenetrationFactorPercents;
			if (OverpenetrationDamage.Overpenetrating)
			{
				overpenetrationFactorPercents = Math.Max(0, OverpenetrationDamage.OverpenetrationFactorPercents - BaseOverpenetrationReduction - num);
			}
			else
			{
				if (num != 0)
				{
					OverpenetrationFactorModifiers.Add(ModifierType.ValAdd, -num, ModifierDescriptor.ArmorAbsorption);
				}
				overpenetrationFactorPercents = Math.Max(0, OverpenetrationFactorModifiers.Apply(OverpenetrationDamage.OverpenetrationFactorPercents));
			}
			OverpenetrationDamage.OverpenetrationFactorPercents = overpenetrationFactorPercents;
		}
	}

	private void SetUnreducedPenetration(MechanicEntity initiator, AbilityData ability)
	{
		if ((bool)initiator.Features.OverpenetrationDoesNotDecreaseDamage)
		{
			OverpenetrationDamage.UnreducedOverpenetrationDamage = true;
		}
		if ((ability?.Blueprint.GetComponent<IgnoreOverpenetrationDamageDecreament>()?.Active(ability.Fact, this, ability)).GetValueOrDefault())
		{
			OverpenetrationDamage.UnreducedOverpenetrationDamage = true;
		}
	}

	public List<RicochetHelper.RicochetTargetData> GetOrCreateRicochetTargets([CanBeNull] CustomGridNodeBase lastHitNode = null)
	{
		if (m_RicochetTargets == null)
		{
			RicochetHelper.RicochetTargetingParameters parameters = new RicochetHelper.RicochetTargetingParameters(MaybeTarget, lastHitNode ?? (MaybeTarget?.CurrentNode.node as CustomGridNodeBase), OverpenetrationDamage, Ability);
			m_RicochetTargets = RicochetHelper.GetPossibleRicochetTargets(in parameters);
		}
		return m_RicochetTargets;
	}

	public List<RicochetHelper.RicochetTargetData> GetOrCreateRicochetTargets(CustomGridNodeBase lastHitNode, HashSet<MechanicEntity> hitTargets, Vector3? preferredRicochetPoint)
	{
		if (m_RicochetTargets == null)
		{
			RicochetHelper.RicochetTargetingParameters parameters = new RicochetHelper.RicochetTargetingParameters(MaybeTarget, lastHitNode, OverpenetrationDamage, Ability, hitTargets, preferredRicochetPoint);
			m_RicochetTargets = RicochetHelper.GetPossibleRicochetTargets(in parameters);
		}
		return m_RicochetTargets;
	}
}
