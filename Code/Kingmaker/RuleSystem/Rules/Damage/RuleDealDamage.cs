using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleDealDamage : RulebookTargetEvent, IDamageHolderRule
{
	[CanBeNull]
	public readonly PartHealth TargetHealth;

	[NotNull]
	public readonly RuleRollDamage RollDamageRule;

	public bool IsCollisionDamage { get; set; }

	public bool IsDot { get; private set; }

	public bool IsIgnorePeacefulZone { get; set; }

	public int HPBeforeDamage { get; private set; }

	public int Result => RollDamageRule.ResultValue;

	public int ResultWithoutReduction => RollDamageRule.ResultValueWithoutReduction;

	public int ResultBeforeDifficulty => RollDamageRule.ResultValueBeforeDifficulty;

	public DamageValue ResultValue => RollDamageRule.Result;

	public bool ResultIsCritical { get; set; }

	public DamageData Damage => RollDamageRule.Damage;

	public BlueprintItemWeapon ContextDamageWeapon { get; set; }

	[CanBeNull]
	public Projectile Projectile { get; set; }

	public bool IsFake { get; }

	[CanBeNull]
	public AbilityData SourceAbility { get; set; }

	[CanBeNull]
	public BlueprintAbilityAreaEffect SourceArea { get; set; }

	public bool DisableFxAndSound { get; set; }

	public RuleDealDamage([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] DamageData damage)
		: this(initiator, target, new RuleRollDamage(initiator, target, damage))
	{
	}

	public RuleDealDamage([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] RuleRollDamage rollDamage)
		: this((MechanicEntity)initiator, (MechanicEntity)target, rollDamage)
	{
	}

	public RuleDealDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] DamageData damage)
		: this(initiator, target, new RuleRollDamage(initiator, target, damage))
	{
	}

	public RuleDealDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] RuleRollDamage rollDamage)
		: base(initiator, target)
	{
		TargetHealth = target.GetHealthOptional();
		IsFake = DamagePolicyContextData.Current == DamagePolicyType.FxOnly;
		RollDamageRule = rollDamage;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(RollDamageRule);
		if (TargetHealth == null)
		{
			return;
		}
		HPBeforeDamage = TargetHealth.HitPointsLeft;
		EventBus.RaiseEvent(delegate(IDamageFXHandler h)
		{
			h.HandleDamageDealt(this);
		});
		if (AbstractUnitCommand.CommandTargetUntargetable((MechanicEntity)base.Initiator, (MechanicEntity)Target, this) || (!IsIgnorePeacefulZone && (bool)Game.Instance.LoadedAreaState.Settings.Peaceful) || (!IsFake && ((MechanicEntity)base.Initiator).IsAttackingGreenNPC((MechanicEntity)Target)))
		{
			return;
		}
		IsDot = base.Reason.Context?.AssociatedBlueprint is BlueprintBuff;
		TargetHealth.LastHandledDamage = this;
		if (IsFake)
		{
			EventBus.RaiseEvent(delegate(IDamageHandler h)
			{
				h.HandleDamageDealt(this);
			});
			return;
		}
		if (Result >= TargetHealth.HitPointsLeft && TrySaveFromDeath())
		{
			TargetHealth.SetHitPointsLeft(1);
		}
		else
		{
			TargetHealth.DealDamage(Result);
		}
		EventBus.RaiseEvent(delegate(IDamageHandler h)
		{
			h.HandleDamageDealt(this);
		});
	}

	private bool TrySaveFromDeath()
	{
		BaseUnitEntity baseUnitEntity = TargetHealth?.Owner as BaseUnitEntity;
		AddResurrectChance.ResurrectChanceUnitPart resurrectChanceUnitPart = baseUnitEntity?.GetOptional<AddResurrectChance.ResurrectChanceUnitPart>();
		if (resurrectChanceUnitPart == null)
		{
			return false;
		}
		return Rulebook.Trigger(new RuleRollChance(baseUnitEntity, resurrectChanceUnitPart.ResurrectChance)).Success;
	}
}
