using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[TypeId("e6da52cf4bc945a1b3276a25991d7a68")]
public class WarhammerAbilityAttackDelivery : AbilityCustomLogic, IAbilityAoEPatternProviderHolder
{
	public enum WeaponAttackType
	{
		None,
		Ranged,
		Melee
	}

	public enum SpecialType
	{
		None,
		Pattern,
		Burst
	}

	[CanBeNull]
	private ScatterPattern m_ScatterPatternCached;

	public WeaponAttackType WeaponAttack;

	public SpecialType Special;

	[SerializeField]
	[ShowIf("ShowIsProjectileField")]
	private bool m_IsProjectile;

	public bool UseBestShootingPosition;

	[Tooltip("If any shot of the attack would hit an ally, that shot instead misses everyone")]
	[ShowIf("IsScatter")]
	public bool ControlledScatter;

	[SerializeField]
	[ShowIf("IsRangedPattern")]
	private bool m_PatternSpreadWithProjectile;

	[InfoBox("Applies damage (1 + AdditionalDamageInstancesCount) times to each target")]
	[SerializeField]
	[ShowIf("IsRangedPattern")]
	public int AdditionalDamageInstancesCount;

	[SerializeField]
	[ShowIf("IsPattern")]
	private AbilityAoEPatternSettings m_PatternSettings;

	[SerializeField]
	[ShowIf("IsWeaponAttack")]
	private bool m_DisableWeaponAttackDamage;

	[SerializeField]
	[ShowIf("IsWeaponAttack")]
	private bool m_DisableDodgeForAlly;

	[SerializeField]
	[ShowIf("IsWeaponAttack")]
	private bool m_DisableOverpenetration;

	[SerializeField]
	[ShowIf("IsWeaponAttack")]
	private bool m_AutoHit;

	[SerializeField]
	[ShowIf("IsScatterOrRangedPattern")]
	private bool m_IsLosDefinedByPattern;

	public bool IsScatter
	{
		get
		{
			if (IsRanged)
			{
				return IsBurst;
			}
			return false;
		}
	}

	public bool IsBurst => Special == SpecialType.Burst;

	public bool IsPattern => Special == SpecialType.Pattern;

	public bool IsRangedPattern
	{
		get
		{
			if (IsRanged)
			{
				return IsPattern;
			}
			return false;
		}
	}

	public bool IsScatterOrRangedPattern
	{
		get
		{
			if (IsRanged)
			{
				if (!IsBurst)
				{
					return IsPattern;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsRanged
	{
		get
		{
			if (WeaponAttack != WeaponAttackType.Ranged)
			{
				return m_IsProjectile;
			}
			return true;
		}
	}

	public bool IsProjectile => m_IsProjectile;

	public bool IsMelee => WeaponAttack == WeaponAttackType.Melee;

	public bool IsWeaponAttack => WeaponAttack != WeaponAttackType.None;

	public bool PatternSpreadWithProjectile => m_PatternSpreadWithProjectile;

	[CanBeNull]
	public IAbilityAoEPatternProvider PatternProvider
	{
		get
		{
			if (!IsPattern)
			{
				return IsScatter ? (m_ScatterPatternCached ?? (m_ScatterPatternCached = new ScatterPattern())) : null;
			}
			return m_PatternSettings;
		}
	}

	private bool ShowIsProjectileField
	{
		get
		{
			if (WeaponAttack == WeaponAttackType.None)
			{
				return !IsScatter;
			}
			return false;
		}
	}

	public bool IsLosDefinedByPattern => m_IsLosDefinedByPattern;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!IsRanged || IsPattern)
		{
			if (!IsPattern)
			{
				return DeliverSimpleImmediately(context, target);
			}
			return DeliverPattern(context, target);
		}
		return DeliverScatter(context, target);
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	private AbilityProjectileAttack DeliverScatter(AbilityExecutionContext context, TargetWrapper target)
	{
		AbilityProjectileAttack abilityProjectileAttack = (IsBurst ? AbilityProjectileAttack.CreateScatter(context, target, context.Ability.BurstAttacksCount, ControlledScatter) : AbilityProjectileAttack.CreateSingleTarget(context, target.Entity, 1));
		if (IsWeaponAttack)
		{
			if (m_AutoHit)
			{
				abilityProjectileAttack.AutoHit();
			}
			if (m_DisableOverpenetration)
			{
				abilityProjectileAttack.DisableOverpenetration();
			}
		}
		else
		{
			abilityProjectileAttack.DisableAttacks();
			abilityProjectileAttack.DisableOverpenetration();
		}
		if (m_DisableWeaponAttackDamage)
		{
			abilityProjectileAttack.DisableWeaponAttackDamage();
		}
		if (m_DisableDodgeForAlly)
		{
			abilityProjectileAttack.DisableDodgeForAlly();
		}
		return abilityProjectileAttack;
	}

	private AbilityAoEPatternAttack DeliverPattern(AbilityExecutionContext context, TargetWrapper target)
	{
		CustomGridNodeBase nearestNodeXZUnwalkable = target.Point.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase customGridNodeBase = (UseBestShootingPosition ? LosCalculations.GetBestShootingNode(context.Caster.CurrentUnwalkableNode, context.Caster.SizeRect, nearestNodeXZUnwalkable, target.SizeRect) : context.Caster.CurrentUnwalkableNode);
		CustomGridNodeBase actualCastNode = AoEPatternHelper.GetActualCastNode(context.Caster, customGridNodeBase.Vector3Position, target.Point);
		AbilityAoEPatternAttack abilityAoEPatternAttack = new AbilityAoEPatternAttack(context, this, PartAbilityPatternSettings.GetAbilityPatternSettings(context.Ability, m_PatternSettings), customGridNodeBase, actualCastNode);
		Vector3? vector = abilityAoEPatternAttack.Context.Pattern.ApplicationNode?.Vector3Position;
		TrySpawnAreaEffect(context, vector.HasValue ? ((TargetWrapper)vector.GetValueOrDefault()) : target);
		if (m_DisableWeaponAttackDamage)
		{
			abilityAoEPatternAttack.DisableWeaponAttackDamage();
		}
		if (m_DisableDodgeForAlly)
		{
			abilityAoEPatternAttack.DisableDodgeForAlly();
		}
		return abilityAoEPatternAttack;
	}

	public static void TrySpawnAreaEffect(AbilityExecutionContext context, TargetWrapper target)
	{
		context.AbilityBlueprint.GetComponent<WarhammerAreaEffectSimultaneousWithAttack>()?.SpawnAreaEffect(context, target);
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverSimpleImmediately(AbilityExecutionContext context, TargetWrapper target)
	{
		if (target.Entity == null || context.Ability.IsValidTargetForAttack(target.Entity))
		{
			RulePerformAttack rulePerformAttack = null;
			if (target.Entity != null)
			{
				rulePerformAttack = new RulePerformAttack(context.Caster, target.Entity, context.Ability, 0, m_DisableWeaponAttackDamage, m_DisableDodgeForAlly)
				{
					AdditionalDamageInstancesCount = AdditionalDamageInstancesCount
				};
				Rulebook.Trigger(rulePerformAttack);
			}
			yield return new AbilityDeliveryTarget(target)
			{
				AttackRule = rulePerformAttack
			};
		}
	}
}
