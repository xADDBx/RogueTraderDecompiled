using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Tutorial.Solvers;

[ClassInfoBox("`t|SolutionUnit`, `t|SolutionItem`")]
[TypeId("c37574e189694141ae4d374e5a69a616")]
public class TutorialSolverBestWeaponAgainstTarget : TutorialSolver
{
	public bool CheckRegeneration;

	public bool CheckEnchantment;

	[InfoBox(Text = "Considers only weapons with at least one enchantment from list")]
	[SerializeField]
	[ShowIf("CheckEnchantment")]
	private BlueprintWeaponEnchantmentReference[] m_Enchantments;

	public ReferenceArrayProxy<BlueprintWeaponEnchantment> Enchantments
	{
		get
		{
			BlueprintReference<BlueprintWeaponEnchantment>[] enchantments = m_Enchantments;
			return enchantments;
		}
	}

	public override bool Solve(TutorialContext context)
	{
		BaseUnitEntity targetUnit = context.TargetUnit;
		UnitHelper.DamageEstimate damageEstimate = default(UnitHelper.DamageEstimate);
		ItemEntityWeapon itemEntityWeapon = null;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			bool shouldCheckUnarmedStrike = false;
			foreach (HandSlot item2 in EnumerateHands(item))
			{
				if (item2.MaybeItem is ItemEntityWeapon itemEntityWeapon2 && (!CheckEnchantment || HasAnyEnchantment(itemEntityWeapon2)))
				{
					UnitHelper.DamageEstimate damageEstimate2 = EstimateWeaponDamage(itemEntityWeapon2, targetUnit, ref shouldCheckUnarmedStrike);
					if ((!CheckRegeneration || CanDisableRegeneration(damageEstimate2.Chunks, targetUnit)) && damageEstimate2.Value > damageEstimate.Value && damageEstimate2.BypassDR)
					{
						damageEstimate = damageEstimate2;
						itemEntityWeapon = itemEntityWeapon2;
					}
				}
			}
		}
		int num;
		if (itemEntityWeapon != null)
		{
			num = ((itemEntityWeapon != context.SourceItem) ? 1 : 0);
			if (num != 0)
			{
				context.SolutionUnit = (itemEntityWeapon.Wielder as BaseUnitEntity) ?? context.SourceUnit;
				context.SolutionItem = itemEntityWeapon;
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool HasAnyEnchantment(ItemEntityWeapon weapon)
	{
		foreach (BlueprintWeaponEnchantment enchantment in Enchantments)
		{
			if (weapon.HasEnchantment(enchantment))
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerable<HandSlot> EnumerateHands(BaseUnitEntity unit)
	{
		for (int i = 0; i < unit.Body.HandsEquipmentSets.Count; i++)
		{
			yield return unit.Body.HandsEquipmentSets[i].PrimaryHand;
			yield return unit.Body.HandsEquipmentSets[i].SecondaryHand;
		}
	}

	private static UnitHelper.DamageEstimate EstimateWeaponDamage(ItemEntityWeapon weapon, BaseUnitEntity target, ref bool shouldCheckUnarmedStrike)
	{
		return UnitHelper.EstimateDamage(weapon, target);
	}

	private static bool CanDisableRegeneration(DamageData[] damageChunks, BaseUnitEntity target)
	{
		return true;
	}
}
