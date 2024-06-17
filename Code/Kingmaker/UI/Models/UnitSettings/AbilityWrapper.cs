using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.UnitLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;

namespace Kingmaker.UI.Models.UnitSettings;

internal struct AbilityWrapper
{
	[CanBeNull]
	public readonly SpellSlot SpellSlot;

	[CanBeNull]
	public readonly AbilityData SpontaneousSpell;

	[CanBeNull]
	public readonly Ability Ability;

	[CanBeNull]
	public readonly ActivatableAbility ActivatableAbility;

	public BlueprintUnitFact Blueprint
	{
		get
		{
			SpellSlot spellSlot = SpellSlot;
			object obj;
			if (spellSlot == null)
			{
				obj = null;
			}
			else
			{
				AbilityData spell = spellSlot.Spell;
				obj = (((object)spell != null) ? SimpleBlueprintExtendAsObject.Or(spell.Blueprint, null) : null);
			}
			if (obj == null)
			{
				AbilityData spontaneousSpell = SpontaneousSpell;
				obj = (((object)spontaneousSpell != null) ? SimpleBlueprintExtendAsObject.Or(spontaneousSpell.Blueprint, null) : null);
				if (obj == null)
				{
					Ability ability = Ability;
					obj = ((ability != null) ? SimpleBlueprintExtendAsObject.Or(ability.Blueprint, null) : null);
					if (obj == null)
					{
						ActivatableAbility activatableAbility = ActivatableAbility;
						if (activatableAbility == null)
						{
							return null;
						}
						obj = SimpleBlueprintExtendAsObject.Or(activatableAbility.Blueprint, null);
					}
				}
			}
			return (BlueprintUnitFact)obj;
		}
	}

	public ItemEntity SourceItem => (ItemEntity)(Ability?.SourceItem);

	public AbilityWrapper([NotNull] SpellSlot spellSlot)
	{
		this = default(AbilityWrapper);
		SpellSlot = spellSlot;
	}

	public AbilityWrapper([NotNull] AbilityData spontaneousSpell)
	{
		this = default(AbilityWrapper);
		SpontaneousSpell = spontaneousSpell;
	}

	public AbilityWrapper([NotNull] Ability ability)
	{
		this = default(AbilityWrapper);
		Ability = ability;
	}

	public AbilityWrapper([NotNull] ActivatableAbility activatableAbility)
	{
		this = default(AbilityWrapper);
		ActivatableAbility = activatableAbility;
	}

	public MechanicActionBarSlot CreateSlot(BaseUnitEntity unit)
	{
		if (SpellSlot != null)
		{
			return new MechanicActionBarSlotMemorizedSpell(SpellSlot)
			{
				Unit = unit
			};
		}
		if (SpontaneousSpell != null)
		{
			return new MechanicActionBarSlotSpontaneousSpell(SpontaneousSpell)
			{
				Unit = unit
			};
		}
		if (Ability != null)
		{
			return new MechanicActionBarSlotAbility
			{
				Ability = Ability.Data,
				Unit = unit
			};
		}
		if (ActivatableAbility != null)
		{
			return new MechanicActionBarSlotActivableAbility
			{
				ActivatableAbility = ActivatableAbility,
				Unit = unit
			};
		}
		throw new Exception("AbilityWrapper.CreateSlot: invalid state, all variants are null");
	}
}
