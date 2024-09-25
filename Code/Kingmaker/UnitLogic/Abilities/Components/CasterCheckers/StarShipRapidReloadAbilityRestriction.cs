using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("87ccb62e8f0bf2e498c93ba5d4f24b73")]
public class StarShipRapidReloadAbilityRestriction : BlueprintComponent, IAbilityCasterRestriction
{
	public bool AllowPenalted;

	public bool AllowReloadAtAccelerationPhase;

	[ShowIf("AllowReloadAtAccelerationPhase")]
	public StarshipWeaponType ReloadWeapon;

	[SerializeField]
	[ShowIf("AllowReloadAtAccelerationPhase")]
	private BlueprintBuffReference m_ReloadBlockingBuff;

	public BlueprintBuff ReloadBlockingBuff => m_ReloadBlockingBuff?.Get();

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity starshipEntity))
		{
			return false;
		}
		if (starshipEntity.Navigation.IsAccelerationMovementPhase && AllowReloadAtAccelerationPhase)
		{
			if (starshipEntity.Buffs.Contains(ReloadBlockingBuff))
			{
				return false;
			}
			return starshipEntity.Hull.WeaponSlots.Where(delegate(WeaponSlot slot)
			{
				if (slot.Weapon.Blueprint.WeaponType == ReloadWeapon)
				{
					ItemEntity sourceItem = slot.ActiveAbility.Data.SourceItem;
					if (sourceItem == null)
					{
						return false;
					}
					return sourceItem.Charges == 0;
				}
				return false;
			}).Any();
		}
		if (starshipEntity.Navigation.IsEndingMovementPhase)
		{
			return starshipEntity.GetOptional<StarShipUnitPartRapidReload>()?.HasWeaponsToReload(AllowPenalted) ?? false;
		}
		return false;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity starshipEntity) || !starshipEntity.Navigation.IsEndingMovementPhase)
		{
			return LocalizedTexts.Instance.Reasons.NotEndingPhase;
		}
		return LocalizedTexts.Instance.Reasons.NoWeaponsUsedDuringPhase;
	}
}
