using System;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Designers.Mechanics.Starships;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueHitPoints : ModifiableValueDependent, IHashable
{
	private Modifier m_BaseStatBonus;

	private Modifier m_DifficultyModifier;

	public override int BaseStatBonus
	{
		get
		{
			if (base.Owner is StarshipEntity)
			{
				return 0;
			}
			int num = base.Container.Owner.GetOptional<PartUnitProgression>()?.CharacterLevel ?? 0;
			bool num2 = base.Owner.GetOptional<PartFaction>()?.IsPlayer ?? false;
			int num3 = ((num >= 35) ? (50 + 2 * (num - 35)) : (15 + num));
			int num4 = base.BaseStat.ModifiedValue / 10;
			UnitEntity obj = base.Owner as UnitEntity;
			UnitDifficultyType? unitDifficultyType = ((obj != null) ? new UnitDifficultyType?(obj.Blueprint.DifficultyType + 1) : null);
			int num5 = ((num2 || !unitDifficultyType.HasValue) ? 2 : Math.Min((int)unitDifficultyType.Value, 3)) * 5;
			int num6 = num4 * num5;
			if (!num2)
			{
				return base.BaseValue * num6 / 100;
			}
			return num3 * num6 / 100;
		}
	}

	protected override int MinValue => 1;

	protected override void UpdateInternalModifiers()
	{
		base.UpdateInternalModifiers();
		if (base.Owner is StarshipEntity initiator)
		{
			m_DifficultyModifier?.Remove();
			float num = SpacecombatDifficultyHelper.HullIntegrityMod(initiator);
			int value = Mathf.RoundToInt((float)base.BaseValue * num) - base.BaseValue;
			m_DifficultyModifier = AddInternalModifier(value, ModifierDescriptor.Difficulty);
			return;
		}
		PartFaction optional = base.Owner.GetOptional<PartFaction>();
		if ((object)optional != null && optional.IsPlayer)
		{
			int num2 = base.Container.Owner.GetOptional<PartUnitProgression>()?.CharacterLevel ?? 0;
			int num3 = ((num2 >= 35) ? (50 + 2 * (num2 - 35)) : (15 + num2));
			Modifier baseStatBonus = m_BaseStatBonus;
			if (baseStatBonus == null || baseStatBonus.ModValue != num3)
			{
				m_BaseStatBonus?.Remove();
				m_BaseStatBonus = AddInternalModifier(num3, StatType.HitPoints, ModifierDescriptor.BaseValue);
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
