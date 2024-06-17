using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers;

public static class AbilitiesHelper
{
	[Serializable]
	public class AbilityDescription
	{
		public bool AbilityFromList;

		[ShowIf("AbilityFromList")]
		[SerializeField]
		[FormerlySerializedAs("AllowedAbilities")]
		private BlueprintAbilityReference[] m_AllowedAbilities;

		[HideIf("AbilityFromList")]
		public SpellDescriptorWrapper SpellDescriptor;

		public ReferenceArrayProxy<BlueprintAbility> AllowedAbilities
		{
			get
			{
				BlueprintReference<BlueprintAbility>[] allowedAbilities = m_AllowedAbilities;
				return allowedAbilities;
			}
		}

		public override string ToString()
		{
			if (!AbilityFromList)
			{
				return SpellDescriptor.ToString();
			}
			return string.Join(", ", AllowedAbilities.Select((BlueprintAbility a) => a.ToString()));
		}
	}

	public static IAbilityAoERadiusProvider GetAoERadiusProvider(this BlueprintAbility ability)
	{
		return GetAppropriateComponent<IAbilityAoERadiusProvider>(ability);
	}

	private static TDefault GetAppropriateComponent<TDefault>(BlueprintAbility ability) where TDefault : class
	{
		if (ability == null)
		{
			return null;
		}
		for (int i = 0; i < ability.ComponentsArray.Length; i++)
		{
			if (ability.ComponentsArray[i] is TDefault result)
			{
				return result;
			}
		}
		return null;
	}

	public static bool PartyUseAbility(AbilityDescription description, bool allowItems, bool spend)
	{
		AbilityData ability = null;
		bool flag = false;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.State.IsHelpless)
			{
				continue;
			}
			foreach (Ability ability3 in item.Abilities)
			{
				if ((ability3.SourceItem == null || !ability3.SourceItem.IsSpendCharges) && IsSuitableAbility(ability3.Data, description) && ability3.Data.IsAvailable)
				{
					ability = ability3.Data;
				}
			}
			if (ability != null)
			{
				break;
			}
		}
		if (ability == null)
		{
			for (int i = 1; i <= 10; i++)
			{
				foreach (BaseUnitEntity item2 in Game.Instance.Player.Party)
				{
					if (!item2.State.IsHelpless)
					{
						ability = GetSuitableAbility(item2, description, i);
						if (ability != null)
						{
							break;
						}
					}
				}
				if (ability != null)
				{
					break;
				}
			}
		}
		if (ability == null && allowItems)
		{
			foreach (BaseUnitEntity item3 in Game.Instance.Player.Party)
			{
				if (item3.State.IsHelpless)
				{
					continue;
				}
				foreach (Ability ability4 in item3.Abilities)
				{
					if (ability4.SourceItem != null && IsSuitableAbility(ability4.Data, description) && ability4.Data.IsAvailable)
					{
						ability = ability4.Data;
					}
				}
				if (ability != null)
				{
					break;
				}
			}
			if (ability == null)
			{
				foreach (ItemEntityUsable item4 in Game.Instance.Player.Inventory.Items.OfType<ItemEntityUsable>())
				{
					if (item4.IsUsableFromInventory && IsSuitableAbility(item4.Blueprint.Abilities.FirstOrDefault(), description))
					{
						BaseUnitEntity bestAvailableUser = item4.GetBestAvailableUser();
						if (bestAvailableUser != null && !spend)
						{
							return true;
						}
						Ability ability2 = bestAvailableUser?.Abilities.Add(item4.Blueprint.Abilities.FirstOrDefault());
						if (ability2 != null)
						{
							flag = true;
							ability2.AddSource(item4);
							ability = ability2.Data;
							break;
						}
					}
				}
			}
		}
		if (ability != null && spend)
		{
			try
			{
				ability.Spend();
				EventBus.RaiseEvent(delegate(IPartyUseAbilityHandler h)
				{
					h.HandleUseAbility(ability);
				});
			}
			finally
			{
				if (flag)
				{
					ability.Caster.Facts.Remove(ability.Fact);
				}
			}
		}
		return ability != null;
	}

	[CanBeNull]
	private static AbilityData GetSuitableAbility(BaseUnitEntity unit, AbilityDescription description, int level)
	{
		return null;
	}

	private static bool IsSuitableAbility(AbilityData ability, AbilityDescription description)
	{
		return IsSuitableAbility(ability.Blueprint, description);
	}

	private static bool IsSuitableAbility(BlueprintAbility ability, AbilityDescription description)
	{
		if (description.AbilityFromList && description.AllowedAbilities.Contains(ability))
		{
			return true;
		}
		if (!description.AbilityFromList && ability.SpellDescriptor.HasAnyFlag(description.SpellDescriptor))
		{
			return true;
		}
		return false;
	}
}
