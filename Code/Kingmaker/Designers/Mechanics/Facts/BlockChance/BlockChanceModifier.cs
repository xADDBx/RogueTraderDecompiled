using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules.Block;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.BlockChance;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("1fc18e3e00c54762831143dd720ad8c8")]
public abstract class BlockChanceModifier : MechanicEntityFactComponentDelegate, IHashable
{
	[Flags]
	public enum PropertyType
	{
		BlockChance = 1,
		BlockChanceMultiplier = 2
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsDropdown]
	public PropertyType Properties = PropertyType.BlockChance;

	[ShowIf("HasBlockChance")]
	public ContextValue BlockChance;

	[ShowIf("HasBlockChanceMultiplier")]
	public ContextValue BlockChanceMultiplierValue = 1;

	private bool HasBlockChance => (Properties & PropertyType.BlockChance) != 0;

	private bool HasBlockChanceMultiplier => (Properties & PropertyType.BlockChanceMultiplier) != 0;

	protected void TryApply(RuleCalculateBlockChance rule)
	{
		if (Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			if (HasBlockChance)
			{
				rule.BlockValueModifiers.Add(BlockChance.Calculate(base.Context), base.Fact);
			}
			if (HasBlockChanceMultiplier)
			{
				rule.BlockValueMultipliers.Add(ModifierType.PctMul_Extra, BlockChanceMultiplierValue.Calculate(base.Context), base.Fact);
			}
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
