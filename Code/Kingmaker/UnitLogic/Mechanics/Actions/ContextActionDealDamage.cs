using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("bdc93cbacbdc05843a933659f15c1302")]
public class ContextActionDealDamage : ContextAction
{
	public struct DamageInfo
	{
		public int Min;

		public int Max;

		public int? PreRolledValue;
	}

	[Header("Damage")]
	public DamageTypeDescription DamageType;

	public bool UseDiceForDamage;

	public bool ReadPreRolledFromSharedValue;

	[ShowIf("ReadPreRolledFromSharedValue")]
	public AbilitySharedValue PreRolledSharedValue;

	[ShowIf("ShowDiceDamageValue")]
	public ContextDiceValue Value;

	[ShowIf("ShowRangeDamageValue")]
	public ContextValue MinDamage;

	[ShowIf("ShowRangeDamageValue")]
	public ContextValue MaxDamage;

	[ShowIf("ShowRangeDamageValue")]
	public ContextValue BonusDamage;

	public ContextDiceValue Penetration;

	[Header("Misc")]
	public bool Half;

	[HideIf("ReadPreRolledFromSharedValue")]
	public bool IsAoE;

	[SerializeField]
	private BlueprintItemWeaponReference m_Weapon;

	public bool WriteResultToSharedValue;

	[ShowIf("WriteResultToSharedValue")]
	public AbilitySharedValue ResultSharedValue;

	public bool DoNotUseCrModifier;

	private bool? m_IsAOE;

	public BlueprintItemWeapon Weapon => m_Weapon?.Get();

	private bool ShowDiceDamageValue
	{
		get
		{
			if (!ReadPreRolledFromSharedValue)
			{
				return UseDiceForDamage;
			}
			return false;
		}
	}

	private bool ShowRangeDamageValue
	{
		get
		{
			if (!ReadPreRolledFromSharedValue)
			{
				return !UseDiceForDamage;
			}
			return false;
		}
	}

	public override string GetCaption()
	{
		string arg = (UseDiceForDamage ? Value.ToString() : string.Format("[{0} to {1}{2}]", MinDamage, MaxDamage, BonusDamage.IsZero ? "" : $" (+{BonusDamage})"));
		string text = (Half ? $"Deal half {arg} of {DamageType}" : $"Deal {arg} of {DamageType}");
		if (IsAoE)
		{
			text += " (AoE)";
		}
		if (WriteResultToSharedValue)
		{
			text += $" >> {ResultSharedValue}";
		}
		return text;
	}

	protected override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
			return;
		}
		DamageInfo damageInfo = GetDamageInfo(base.Context);
		int value = DealHitPointsDamage(damageInfo);
		if (WriteResultToSharedValue)
		{
			base.Context[ResultSharedValue] = value;
		}
	}

	private DamageInfo GetDamageInfo(MechanicsContext context)
	{
		int min = (UseDiceForDamage ? (Value.DiceCountValue.Calculate(context) + Value.BonusValue.Calculate(context)) : (MinDamage.Calculate(context) + BonusDamage.Calculate(context)));
		int max = (UseDiceForDamage ? (Value.DiceCountValue.Calculate(context) * Value.DiceType.Sides() + Value.BonusValue.Calculate(context)) : (MaxDamage.Calculate(context) + BonusDamage.Calculate(context)));
		DamageInfo result = default(DamageInfo);
		result.Min = min;
		result.Max = max;
		result.PreRolledValue = (ReadPreRolledFromSharedValue ? new int?(context[PreRolledSharedValue]) : null);
		return result;
	}

	private int DealHitPointsDamage(DamageInfo info)
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			Element.LogError(this, "Caster is missing");
			return 0;
		}
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			Element.LogError(this, "Target is missing");
			return 0;
		}
		int min = info.Min;
		int max = info.Max;
		int value = Penetration?.Calculate(base.Context) ?? 0;
		int value2 = 0;
		CustomGridNodeBase customGridNodeBase = base.AbilityContext?.Pattern.ApplicationNode;
		if (customGridNodeBase != null)
		{
			value2 = entity.DistanceToInCells(customGridNodeBase.Vector3Position);
		}
		DamageData resultDamage = new CalculateDamageParams(maybeCaster, entity, base.AbilityContext?.Ability, null, DamageType.CreateDamage(min, max), value, value2, DoNotUseCrModifier).Trigger().ResultDamage;
		resultDamage.CalculatedValue = info.PreRolledValue;
		RuleDealDamage ruleDealDamage = new RuleDealDamage(maybeCaster, entity, resultDamage)
		{
			Projectile = base.Projectile,
			SourceAbility = (base.AbilityContext?.Ability ?? base.Context.SourceAbilityContext?.Ability),
			SourceArea = (base.Context.AssociatedBlueprint as BlueprintAbilityAreaEffect),
			ContextDamageWeapon = Weapon
		};
		base.Context.TriggerRule(ruleDealDamage);
		return ruleDealDamage.Result;
	}

	private bool CalculateIsAOE()
	{
		BlueprintAbilityAreaEffect blueprintAbilityAreaEffect = base.Context.AssociatedBlueprint as BlueprintAbilityAreaEffect;
		AbilityExecutionContext abilityContext = base.AbilityContext;
		bool flag = ((abilityContext == null) ? null : SimpleBlueprintExtendAsObject.Or(abilityContext.AbilityBlueprint, null)?.ComponentsArray)?.HasItem((BlueprintComponent c) => c is AbilityTargetsInPattern) ?? false;
		m_IsAOE = IsAoE || blueprintAbilityAreaEffect != null || flag;
		return m_IsAOE.Value;
	}

	public DamagePredictionData GetDamagePrediction([NotNull] AbilityExecutionContext context, [CanBeNull] MechanicEntity target)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			AbilityData ability = context.Ability;
			DamageInfo damageInfo = GetDamageInfo(context);
			int min = damageInfo.Min;
			int max = damageInfo.Max;
			DamageData baseDamageOverride = DamageType.CreateDamage(min, max);
			int num = Penetration?.Calculate(context) ?? 0;
			baseDamageOverride = new CalculateDamageParams(ability.Caster, target, ability, null, baseDamageOverride, num).Trigger().ResultDamage;
			return new DamagePredictionData
			{
				MinDamage = baseDamageOverride.MinValue,
				MaxDamage = baseDamageOverride.MaxValue,
				Penetration = num
			};
		}
	}
}
