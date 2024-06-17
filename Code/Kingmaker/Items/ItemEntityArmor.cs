using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items;

public class ItemEntityArmor : ItemEntity<BlueprintItemArmor>, IHashable
{
	public static readonly StatType[] PenaltyDependentSkills = new StatType[0];

	public ItemEntityShield Shield { get; private set; }

	public override bool IsPartOfAnotherItem => Shield != null;

	public ItemEntityArmor([NotNull] BlueprintItemArmor bpItem, ItemEntityShield shield = null)
		: base(bpItem)
	{
		Shield = shield;
	}

	protected ItemEntityArmor(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override bool CanBeEquippedInternal(MechanicEntity owner)
	{
		if (base.CanBeEquippedInternal(owner))
		{
			return owner.GetProficienciesOptional()?.Contains(base.Blueprint.ProficiencyGroup) ?? true;
		}
		return false;
	}

	public bool IsMithral()
	{
		return false;
	}

	public ArmorProficiencyGroup ArmorType()
	{
		if (!IsMithral())
		{
			return base.Blueprint.ProficiencyGroup;
		}
		if (base.Blueprint.ProficiencyGroup != ArmorProficiencyGroup.Power)
		{
			if (base.Blueprint.ProficiencyGroup != ArmorProficiencyGroup.Heavy)
			{
				if (base.Blueprint.ProficiencyGroup != ArmorProficiencyGroup.Medium)
				{
					return base.Blueprint.ProficiencyGroup;
				}
				return ArmorProficiencyGroup.Light;
			}
			return ArmorProficiencyGroup.Medium;
		}
		return ArmorProficiencyGroup.Heavy;
	}

	public void PostLoad(ItemEntityShield shield)
	{
		Shield = shield;
		PostLoad();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
