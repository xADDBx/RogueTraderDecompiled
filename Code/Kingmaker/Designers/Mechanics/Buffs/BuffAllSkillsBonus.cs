using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[ComponentName("BuffMechanics/Bonus to all skills")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("1d52ceca07db9ed4a98df9782359f75b")]
public class BuffAllSkillsBonus : UnitFactComponentDelegate, IHashable
{
	public ModifierDescriptor Descriptor;

	public int Value;

	public ContextValue Multiplier;

	protected override void OnActivateOrPostLoad()
	{
		int value = Multiplier.Calculate(base.Context) * Value;
		StatType[] skills = StatTypeHelper.Skills;
		foreach (StatType type in skills)
		{
			base.Owner.Stats.GetStat<ModifiableValue>(type).AddModifier(value, base.Runtime, Descriptor);
		}
	}

	protected override void OnDeactivate()
	{
		StatType[] skills = StatTypeHelper.Skills;
		foreach (StatType type in skills)
		{
			base.Owner.Stats.GetStat<ModifiableValue>(type)?.RemoveModifiersFrom(base.Runtime);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
