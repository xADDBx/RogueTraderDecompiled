using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartAbilityCooldowns : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartAbilityCooldowns>, IEntityPartOwner
	{
		PartAbilityCooldowns AbilityCooldowns { get; }
	}

	public class CooldownData : IHashable
	{
		[JsonProperty]
		public int Cooldown;

		[JsonProperty]
		public bool UntilEndOfCombat;

		[JsonProperty]
		public bool Interrupt;

		[JsonConstructor]
		private CooldownData()
		{
		}

		public CooldownData(int cooldown, bool untilEndOfCombat = false)
		{
			Cooldown = cooldown;
			UntilEndOfCombat = untilEndOfCombat;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref Cooldown);
			result.Append(ref UntilEndOfCombat);
			result.Append(ref Interrupt);
			return result;
		}
	}

	[Serializable]
	public struct CooldownsStateSave : IHashable
	{
		public Dictionary<BlueprintAbility, CooldownData> AbilityCooldowns;

		public Dictionary<BlueprintAbilityGroup, CooldownData> GroupCooldowns;

		public Hash128 GetHash128()
		{
			return default(Hash128);
		}
	}

	[JsonProperty]
	private Dictionary<BlueprintAbility, CooldownData> m_AbilityCooldowns = new Dictionary<BlueprintAbility, CooldownData>();

	[JsonProperty]
	private Dictionary<BlueprintAbilityGroup, CooldownData> m_GroupCooldowns = new Dictionary<BlueprintAbilityGroup, CooldownData>();

	[JsonProperty]
	private List<CooldownsStateSave> m_SavedCooldowns = new List<CooldownsStateSave>();

	[JsonProperty]
	public RestrictionCalculator InterruptionAbilityRestrictions;

	[JsonConstructor]
	public PartAbilityCooldowns()
	{
	}

	public void StartCooldown(AbilityData ability)
	{
		if (!base.Owner.IsInCombat)
		{
			return;
		}
		if (IsOnCooldown(ability))
		{
			PFLog.Default.Error(ability.Blueprint.name + " is already on cooldown!");
			return;
		}
		RuleCalculateCooldown ruleCalculateCooldown = Rulebook.Trigger(new RuleCalculateCooldown(base.Owner, ability));
		if (ruleCalculateCooldown.Result <= 0)
		{
			WarhammerCooldown cooldownComponent = ruleCalculateCooldown.CooldownComponent;
			if (cooldownComponent == null || !cooldownComponent.UntilEndOfCombat)
			{
				goto IL_00d0;
			}
		}
		CooldownData value = new CooldownData(ruleCalculateCooldown.Result, ruleCalculateCooldown.CooldownComponent?.UntilEndOfCombat ?? false)
		{
			Interrupt = (base.Owner.Initiative.InterruptingOrder > 0)
		};
		m_AbilityCooldowns.Add(ability.Blueprint, value);
		goto IL_00d0;
		IL_00d0:
		if (ability.AbilityGroups.Count > 0)
		{
			foreach (BlueprintAbilityGroup abilityGroup in ability.AbilityGroups)
			{
				StartGroupCooldown(abilityGroup, ruleCalculateCooldown);
			}
		}
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitAbilityCooldownHandler>)delegate(IUnitAbilityCooldownHandler h)
		{
			h.HandleAbilityCooldownStarted(ability);
		}, isCheckRuntime: true);
	}

	public void StartAutonomousCooldown(BlueprintAbility ability, int rounds)
	{
		if (m_AbilityCooldowns.ContainsKey(ability))
		{
			PFLog.Default.Error(ability.name + " is already on cooldown!");
			return;
		}
		if (rounds < 1)
		{
			PFLog.Default.Error($"Wrong cooldown value for {ability.name} : {rounds}!");
			return;
		}
		CooldownData value = new CooldownData(rounds)
		{
			Interrupt = (base.Owner.Initiative.InterruptingOrder > 0)
		};
		m_AbilityCooldowns.Add(ability, value);
	}

	public int? GetAutonomousCooldown(BlueprintAbility ability)
	{
		if (m_AbilityCooldowns.TryGetValue(ability, out var value))
		{
			if (value.UntilEndOfCombat)
			{
				return 99;
			}
			return value.Cooldown;
		}
		return null;
	}

	public void RemoveAutonomousCooldown(BlueprintAbility ability)
	{
		if (!m_AbilityCooldowns.ContainsKey(ability))
		{
			PFLog.Default.Error(ability.name + " is not on cooldown!");
		}
		else
		{
			m_AbilityCooldowns.Remove(ability);
		}
	}

	public void StartGroupCooldown(BlueprintAbilityGroup abilityGroup, RuleCalculateCooldown cooldownRule = null)
	{
		if (abilityGroup == null)
		{
			PFLog.Default.Error("AbilityGroup is null!");
			return;
		}
		if (m_GroupCooldowns.ContainsKey(abilityGroup))
		{
			PFLog.Default.Error(abilityGroup.name + " is already on cooldown!");
			return;
		}
		int num = 0;
		num = ((cooldownRule != null) ? (cooldownRule.GroupCooldownsData.FirstOrDefault((GroupCooldownData p) => p.Group == abilityGroup)?.Cooldown ?? 0) : Rulebook.Trigger(new RuleCalculateGroupCooldown(base.Owner, abilityGroup)).Result);
		if (num > 0)
		{
			CooldownData value = new CooldownData(num)
			{
				Interrupt = (base.Owner.Initiative.InterruptingOrder > 0)
			};
			m_GroupCooldowns.Add(abilityGroup, value);
		}
	}

	public bool IsOnCooldown(AbilityData ability)
	{
		if (!base.Owner.IsInCombat)
		{
			return false;
		}
		if (m_AbilityCooldowns.ContainsKey(ability.Blueprint))
		{
			return true;
		}
		if (ability.AbilityGroups.Count > 0)
		{
			foreach (BlueprintAbilityGroup abilityGroup in ability.AbilityGroups)
			{
				if (abilityGroup != null && m_GroupCooldowns.ContainsKey(abilityGroup) && !IsIgnoredByComponent(abilityGroup, ability))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsOnCooldownUntilEndOfCombat(AbilityData ability)
	{
		if (!base.Owner.IsInCombat)
		{
			return false;
		}
		if (m_AbilityCooldowns.TryGetValue(ability.Blueprint, out var value) && value.UntilEndOfCombat)
		{
			return true;
		}
		if (ability.AbilityGroups.Count > 0)
		{
			foreach (BlueprintAbilityGroup abilityGroup in ability.AbilityGroups)
			{
				if (abilityGroup != null && m_GroupCooldowns.TryGetValue(abilityGroup, out var value2) && value2.UntilEndOfCombat && !IsIgnoredByComponent(abilityGroup, ability))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetCooldown(BlueprintAbility ability)
	{
		if (!base.Owner.IsInCombat)
		{
			return 0;
		}
		int num = 0;
		if (m_AbilityCooldowns.TryGetValue(ability, out var value))
		{
			num = (value.UntilEndOfCombat ? int.MaxValue : value.Cooldown);
		}
		AbilityData data = ((Ability)base.Owner.Facts.Get(ability)).Data;
		if (data.AbilityGroups.Count > 0)
		{
			foreach (BlueprintAbilityGroup abilityGroup in ability.AbilityGroups)
			{
				if (abilityGroup != null && m_GroupCooldowns.TryGetValue(abilityGroup, out var value2) && !IsIgnoredByComponent(abilityGroup, data))
				{
					num = Math.Max(value2.UntilEndOfCombat ? int.MaxValue : value2.Cooldown, num);
				}
			}
		}
		return num;
	}

	public bool IsIgnoredByComponent(BlueprintAbilityGroup group, AbilityData ability)
	{
		return ability.Blueprint.GetComponents<WarhammerIgnoreGroupCooldownByBuff>().Any((WarhammerIgnoreGroupCooldownByBuff component) => component.IgnoreGroup == group && ability.Caster?.Buffs.GetBuff(component.IgnoreBuff) != null);
	}

	public void TickCooldowns(bool interrupt)
	{
		KeyValuePair<BlueprintAbility, CooldownData>[] array = m_AbilityCooldowns.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<BlueprintAbility, CooldownData> keyValuePair = array[i];
			if (interrupt && !keyValuePair.Value.Interrupt)
			{
				continue;
			}
			keyValuePair.Value.Interrupt = false;
			if (keyValuePair.Value.UntilEndOfCombat)
			{
				if (!base.Owner.IsInCombat)
				{
					m_AbilityCooldowns.Remove(keyValuePair.Key);
				}
				continue;
			}
			keyValuePair.Value.Cooldown--;
			if (keyValuePair.Value.Cooldown <= 0)
			{
				m_AbilityCooldowns.Remove(keyValuePair.Key);
			}
		}
		KeyValuePair<BlueprintAbilityGroup, CooldownData>[] array2 = m_GroupCooldowns.ToArray();
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<BlueprintAbilityGroup, CooldownData> keyValuePair2 = array2[i];
			if (interrupt && !keyValuePair2.Value.Interrupt)
			{
				continue;
			}
			keyValuePair2.Value.Interrupt = false;
			if (keyValuePair2.Value.UntilEndOfCombat)
			{
				if (!base.Owner.IsInCombat)
				{
					m_GroupCooldowns.Remove(keyValuePair2.Key);
				}
				continue;
			}
			keyValuePair2.Value.Cooldown--;
			if (keyValuePair2.Value.Cooldown <= 0)
			{
				m_GroupCooldowns.Remove(keyValuePair2.Key);
			}
		}
	}

	public void RemoveGroupCooldown(BlueprintAbilityGroup abilityGroup)
	{
		if (abilityGroup == null)
		{
			PFLog.Default.Error("Ability group is null!");
			return;
		}
		foreach (BlueprintAbilityGroup group in abilityGroup.GetAllAbilityGroups())
		{
			m_GroupCooldowns.Remove(group);
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitAbilityCooldownHandler>)delegate(IUnitAbilityCooldownHandler h)
			{
				h.HandleGroupCooldownRemoved(group);
			}, isCheckRuntime: true);
		}
	}

	public void RemoveHandAbilityGroupsCooldown()
	{
		BlueprintCombatRoot combatRoot = BlueprintWarhammerRoot.Instance.CombatRoot;
		RemoveGroupCooldown(combatRoot.PrimaryHandAbilityGroup);
		RemoveGroupCooldown(combatRoot.SecondaryHandAbilityGroup);
	}

	public void ResetCooldowns(bool ignoreOncePerCombatRestriction = false)
	{
		if (ignoreOncePerCombatRestriction)
		{
			m_AbilityCooldowns.Clear();
			m_GroupCooldowns.Clear();
		}
		else
		{
			m_AbilityCooldowns = GetUntilEndOfCombatCooldowns();
			m_GroupCooldowns = GetUntilEndOfCombatGroupCooldowns();
		}
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitAbilityCooldownHandler>)delegate(IUnitAbilityCooldownHandler h)
		{
			h.HandleCooldownReset();
		}, isCheckRuntime: true);
	}

	public int GroupCooldown(BlueprintAbilityGroup abilityGroup)
	{
		if (!base.Owner.IsInCombat)
		{
			return 0;
		}
		if (abilityGroup == null)
		{
			PFLog.Default.Error("Ability group is null!");
			return 0;
		}
		int num = 0;
		foreach (BlueprintAbilityGroup allAbilityGroup in abilityGroup.GetAllAbilityGroups())
		{
			num = Math.Max(m_GroupCooldowns.Get(allAbilityGroup)?.Cooldown ?? 0, num);
		}
		return num;
	}

	public bool GroupIsOnCooldown(BlueprintAbilityGroup abilityGroup)
	{
		if (!base.Owner.IsInCombat)
		{
			return false;
		}
		if (abilityGroup == null)
		{
			PFLog.Default.Error("Ability group is null!");
			return false;
		}
		foreach (BlueprintAbilityGroup allAbilityGroup in abilityGroup.GetAllAbilityGroups())
		{
			CooldownData cooldownData = m_GroupCooldowns.Get(allAbilityGroup);
			if (cooldownData != null && cooldownData.Cooldown > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveAbilityCooldown(BlueprintAbility ability)
	{
		if (ability == null)
		{
			PFLog.Default.Error("Ability group is null!");
		}
		else
		{
			m_AbilityCooldowns.Remove(ability);
		}
	}

	public void Clear()
	{
		m_AbilityCooldowns.Clear();
		m_GroupCooldowns.Clear();
	}

	public void SaveCooldownData()
	{
		CooldownsStateSave cooldownsStateSave = default(CooldownsStateSave);
		cooldownsStateSave.AbilityCooldowns = m_AbilityCooldowns.ToDictionary((KeyValuePair<BlueprintAbility, CooldownData> entry) => entry.Key, (KeyValuePair<BlueprintAbility, CooldownData> entry) => entry.Value);
		cooldownsStateSave.GroupCooldowns = m_GroupCooldowns.ToDictionary((KeyValuePair<BlueprintAbilityGroup, CooldownData> entry) => entry.Key, (KeyValuePair<BlueprintAbilityGroup, CooldownData> entry) => entry.Value);
		CooldownsStateSave item = cooldownsStateSave;
		m_SavedCooldowns.Add(item);
	}

	public void RestoreCooldownData(bool ignoreOncePerCombatRestriction = false)
	{
		if (m_SavedCooldowns.Count == 0)
		{
			AILogger.Instance.Error(new AILogMessage("trying to restore cooldowns for " + base.Owner.Name + " but there are none saved"));
			return;
		}
		Dictionary<BlueprintAbility, CooldownData> abilityCooldowns = m_SavedCooldowns.Last().AbilityCooldowns;
		Dictionary<BlueprintAbilityGroup, CooldownData> groupCooldowns = m_SavedCooldowns.Last().GroupCooldowns;
		if (ignoreOncePerCombatRestriction)
		{
			if (abilityCooldowns != null)
			{
				m_AbilityCooldowns = abilityCooldowns;
			}
			else
			{
				m_AbilityCooldowns.Clear();
			}
			if (groupCooldowns != null)
			{
				m_GroupCooldowns = groupCooldowns;
			}
			else
			{
				m_GroupCooldowns.Clear();
			}
		}
		else
		{
			Dictionary<BlueprintAbility, CooldownData> untilEndOfCombatCooldowns = GetUntilEndOfCombatCooldowns();
			if (abilityCooldowns != null)
			{
				m_AbilityCooldowns = abilityCooldowns;
			}
			else
			{
				m_AbilityCooldowns.Clear();
			}
			foreach (KeyValuePair<BlueprintAbility, CooldownData> item in untilEndOfCombatCooldowns.Where((KeyValuePair<BlueprintAbility, CooldownData> cooldown) => !m_AbilityCooldowns.ContainsKey(cooldown.Key)))
			{
				m_AbilityCooldowns.Add(item.Key, item.Value);
			}
			Dictionary<BlueprintAbilityGroup, CooldownData> untilEndOfCombatGroupCooldowns = GetUntilEndOfCombatGroupCooldowns();
			if (groupCooldowns != null)
			{
				m_GroupCooldowns = groupCooldowns;
			}
			else
			{
				m_GroupCooldowns.Clear();
			}
			foreach (KeyValuePair<BlueprintAbilityGroup, CooldownData> item2 in untilEndOfCombatGroupCooldowns.Where((KeyValuePair<BlueprintAbilityGroup, CooldownData> cooldown) => !m_GroupCooldowns.ContainsKey(cooldown.Key)))
			{
				m_GroupCooldowns.Add(item2.Key, item2.Value);
			}
		}
		m_SavedCooldowns.RemoveLast();
	}

	private Dictionary<BlueprintAbility, CooldownData> GetUntilEndOfCombatCooldowns()
	{
		return m_AbilityCooldowns.Where((KeyValuePair<BlueprintAbility, CooldownData> abilityCooldown) => abilityCooldown.Value.UntilEndOfCombat).ToDictionary((KeyValuePair<BlueprintAbility, CooldownData> abilityCooldown) => abilityCooldown.Key, (KeyValuePair<BlueprintAbility, CooldownData> abilityCooldown) => abilityCooldown.Value);
	}

	private Dictionary<BlueprintAbilityGroup, CooldownData> GetUntilEndOfCombatGroupCooldowns()
	{
		return m_GroupCooldowns.Where((KeyValuePair<BlueprintAbilityGroup, CooldownData> groupCooldown) => groupCooldown.Value.UntilEndOfCombat).ToDictionary((KeyValuePair<BlueprintAbilityGroup, CooldownData> groupCooldown) => groupCooldown.Key, (KeyValuePair<BlueprintAbilityGroup, CooldownData> groupCooldown) => groupCooldown.Value);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<BlueprintAbility, CooldownData> abilityCooldowns = m_AbilityCooldowns;
		if (abilityCooldowns != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintAbility, CooldownData> item in abilityCooldowns)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<CooldownData>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		Dictionary<BlueprintAbilityGroup, CooldownData> groupCooldowns = m_GroupCooldowns;
		if (groupCooldowns != null)
		{
			int val5 = 0;
			foreach (KeyValuePair<BlueprintAbilityGroup, CooldownData> item2 in groupCooldowns)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val6);
				Hash128 val7 = ClassHasher<CooldownData>.GetHash128(item2.Value);
				hash2.Append(ref val7);
				val5 ^= hash2.GetHashCode();
			}
			result.Append(ref val5);
		}
		List<CooldownsStateSave> savedCooldowns = m_SavedCooldowns;
		if (savedCooldowns != null)
		{
			for (int i = 0; i < savedCooldowns.Count; i++)
			{
				CooldownsStateSave obj = savedCooldowns[i];
				Hash128 val8 = StructHasher<CooldownsStateSave>.GetHash128(ref obj);
				result.Append(ref val8);
			}
		}
		Hash128 val9 = ClassHasher<RestrictionCalculator>.GetHash128(InterruptionAbilityRestrictions);
		result.Append(ref val9);
		return result;
	}
}
