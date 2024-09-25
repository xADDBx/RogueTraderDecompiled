using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add bonus to all skills")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("b18305c0363e40d4a7f0f5743a583bc5")]
public class AddContextAllSkillBonus : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public int Multiplier = 1;

	public ContextValue Value;

	protected override void OnActivateOrPostLoad()
	{
		if (!Restrictions.IsPassed(base.Fact, base.Context, null, base.Context.SourceAbilityContext?.Ability))
		{
			return;
		}
		foreach (BlueprintSkillAdvancement.SkillType item in Enum.GetValues(typeof(BlueprintSkillAdvancement.SkillType)).Cast<BlueprintSkillAdvancement.SkillType>())
		{
			StatType statTypeBySkill = BlueprintSkillAdvancement.GetStatTypeBySkill(item);
			int value = CalculateValue(base.Context, statTypeBySkill);
			base.Owner.Stats.GetStat(statTypeBySkill, canBeNull: true)?.AddModifier(value, base.Runtime, Descriptor);
		}
	}

	protected override void OnDeactivate()
	{
		foreach (BlueprintSkillAdvancement.SkillType item in Enum.GetValues(typeof(BlueprintSkillAdvancement.SkillType)).Cast<BlueprintSkillAdvancement.SkillType>())
		{
			StatType statTypeBySkill = BlueprintSkillAdvancement.GetStatTypeBySkill(item);
			base.Owner.Stats.GetStat(statTypeBySkill, canBeNull: true)?.RemoveModifiersFrom(base.Runtime);
		}
	}

	public int CalculateValue(MechanicsContext context, StatType stat)
	{
		int value = Value.Calculate(context) * Multiplier;
		return AddStatBonus.TryApplyArcanistPowerfulChange(context, stat, value);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
