using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Block;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("b05459cd70cb4a5e97a3f8527ad3bc37")]
public sealed class BlockChanceGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional
{
	public PropertyTargetType Attacker;

	public bool NoTarget;

	[ShowIf("NoTarget")]
	public bool OnlyFromShield;

	[HideIf("OnlyFromShield")]
	public bool OnlyNegativeModifiers;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!OnlyFromShield)
		{
			if (!NoTarget)
			{
				return "Parry of " + FormulaTargetScope.Current + " against " + Attacker.Colorized();
			}
			return "Parry of " + FormulaTargetScope.Current + " against abstract attack";
		}
		return "Block of equipped shield of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is UnitEntity unitEntity))
		{
			return 0;
		}
		int num = 0;
		UnitPartAeldariShields optional = unitEntity.GetOptional<UnitPartAeldariShields>();
		if (optional != null)
		{
			bool flag = false;
			int num2 = num;
			foreach (Tuple<string, UnitPartAeldariShields.Entry> item in optional.ActiveEntries())
			{
				flag = true;
				num = 0;
				RuleCalculateBlockChance ruleCalculateBlockChance = new RuleCalculateBlockChance(unitEntity, item.Item2.BlockChance, (UnitEntity)this.GetTargetByType(Attacker), this.GetAbility());
				Rulebook.Trigger(ruleCalculateBlockChance);
				if (OnlyNegativeModifiers)
				{
					foreach (Modifier item2 in ruleCalculateBlockChance.BlockValueModifiers.List)
					{
						if (item2.Value < 0)
						{
							num += item2.Value;
						}
					}
					if (!ruleCalculateBlockChance.BlockValueMultipliers.Empty)
					{
						num *= ruleCalculateBlockChance.BlockValueMultipliers.Value;
					}
				}
				else
				{
					num = ruleCalculateBlockChance.Result;
				}
				num2 = Mathf.Max(num2, num);
			}
			if (flag)
			{
				return num2;
			}
		}
		int blockChance = unitEntity.Body.SecondaryHand.MaybeShield?.Blueprint.BlockChance ?? 0;
		if (NoTarget)
		{
			if (OnlyFromShield)
			{
				return (base.CurrentEntity.GetBodyOptional()?.Hands?.FirstOrDefault((HandSlot p) => p.HasShield)?.Shield?.Blueprint?.BlockChance).GetValueOrDefault();
			}
			return Rulebook.Trigger(new RuleCalculateBlockChance(unitEntity, blockChance)).Result;
		}
		RuleCalculateBlockChance ruleCalculateBlockChance2 = new RuleCalculateBlockChance(unitEntity, blockChance, (UnitEntity)this.GetTargetByType(Attacker), this.GetAbility());
		Rulebook.Trigger(ruleCalculateBlockChance2);
		if (OnlyNegativeModifiers)
		{
			foreach (Modifier item3 in ruleCalculateBlockChance2.BlockValueModifiers.List)
			{
				if (item3.Value < 0)
				{
					num += item3.Value;
				}
			}
			if (!ruleCalculateBlockChance2.BlockValueMultipliers.Empty)
			{
				num *= ruleCalculateBlockChance2.BlockValueMultipliers.Value;
			}
		}
		else
		{
			num = ruleCalculateBlockChance2.Result;
		}
		return num;
	}
}
