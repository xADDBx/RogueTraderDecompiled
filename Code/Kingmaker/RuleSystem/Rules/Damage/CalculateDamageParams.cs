using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.RuleSystem.Rules.Damage;

public struct CalculateDamageParams : IEquatable<CalculateDamageParams>
{
	[NotNull]
	public readonly MechanicEntity Initiator;

	[CanBeNull]
	public readonly MechanicEntity Target;

	[CanBeNull]
	public readonly AbilityData Ability;

	[CanBeNull]
	public readonly RulePerformAttackRoll PerformAttackRoll;

	[CanBeNull]
	public readonly DamageData BaseDamageOverride;

	[CanBeNull]
	public readonly int? BasePenetrationOverride;

	[CanBeNull]
	public readonly int? Distance;

	public readonly bool ForceCrit;

	public readonly bool CalculatedOverpenetration;

	public readonly bool DoNotUseCrModifier;

	public bool FakeRule { get; set; }

	public bool HasNoTarget { get; set; }

	public RuleReason? Reason { get; set; }

	public CalculateDamageParams([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] AbilityData ability, [CanBeNull] RulePerformAttackRoll performAttackRoll = null, [CanBeNull] DamageData baseDamageOverride = null, [CanBeNull] int? basePenetrationOverride = null, [CanBeNull] int? distance = null, bool forceCrit = false, bool calculatedOverpenetration = false, bool doNotUseCrModifier = false)
	{
		Initiator = initiator;
		Target = target;
		Ability = ability;
		PerformAttackRoll = performAttackRoll;
		BaseDamageOverride = baseDamageOverride;
		BasePenetrationOverride = basePenetrationOverride;
		Distance = distance;
		ForceCrit = forceCrit;
		DoNotUseCrModifier = doNotUseCrModifier;
		CalculatedOverpenetration = calculatedOverpenetration;
		FakeRule = false;
		HasNoTarget = false;
		Reason = null;
	}

	public bool Equals(CalculateDamageParams other)
	{
		if (Initiator.Equals(other.Initiator) && object.Equals(Target, other.Target) && object.Equals(Ability, other.Ability) && object.Equals(PerformAttackRoll, other.PerformAttackRoll) && object.Equals(BaseDamageOverride, other.BaseDamageOverride) && BasePenetrationOverride == other.BasePenetrationOverride && Distance == other.Distance && ForceCrit == other.ForceCrit && CalculatedOverpenetration == other.CalculatedOverpenetration)
		{
			return DoNotUseCrModifier == other.DoNotUseCrModifier;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is CalculateDamageParams other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(Initiator);
		hashCode.Add(Target);
		hashCode.Add(Ability);
		hashCode.Add(PerformAttackRoll);
		hashCode.Add(BaseDamageOverride);
		hashCode.Add(BasePenetrationOverride);
		hashCode.Add(Distance);
		hashCode.Add(ForceCrit);
		hashCode.Add(CalculatedOverpenetration);
		hashCode.Add(DoNotUseCrModifier);
		return hashCode.ToHashCode();
	}

	public RuleCalculateDamage Trigger()
	{
		return RuleCalculateDamage.Trigger(this);
	}
}
