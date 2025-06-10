using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("3e114442d947481688681f4f6f763922")]
public class WarhammerOverrideAbilityCasterPositionByPet : BlueprintComponent, IAbilityOverrideCasterForRange
{
	[Tooltip("Only override position for Range/Hologram rendering in UI, but not for mechanical range checks.")]
	[SerializeField]
	private bool m_ForUiOnly;

	public MechanicEntity GetCaster(MechanicEntity originalCaster, bool forUi)
	{
		if (m_ForUiOnly && !forUi)
		{
			return null;
		}
		return originalCaster?.GetOptional<UnitPartPetOwner>()?.PetUnit;
	}
}
