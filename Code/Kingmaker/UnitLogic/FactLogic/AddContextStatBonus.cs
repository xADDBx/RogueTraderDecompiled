using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add stat bonus")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("f08844ce14d498a45a9fc64582489a2a")]
public class AddContextStatBonus : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public StatType Stat;

	public int Multiplier = 1;

	public ContextValue Value;

	protected override void OnActivateOrPostLoad()
	{
		if (Restrictions.IsPassed(base.Fact, base.Context, null, base.Context.SourceAbilityContext?.Ability))
		{
			int value = CalculateValue(base.Context);
			base.Owner.Stats.GetStat(Stat, canBeNull: true)?.AddModifier(value, base.Runtime, Descriptor);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Stat, canBeNull: true)?.RemoveModifiersFrom(base.Runtime);
	}

	public int CalculateValue(MechanicsContext context)
	{
		int value = Value.Calculate(context) * Multiplier;
		return AddStatBonus.TryApplyArcanistPowerfulChange(context, Stat, value);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
