using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Models.UnitSettings.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class PartUnitUISettings : BaseUnitPart, IHashable
{
	[JsonProperty]
	private bool m_ShowHelm = true;

	[JsonProperty]
	private bool m_ShowHelmAboveAll = true;

	[JsonProperty]
	private bool m_ShowBackpack = true;

	[JsonProperty]
	private bool m_ShowGloves = true;

	[JsonProperty]
	private bool m_ShowBoots = true;

	[JsonProperty]
	private bool m_ShowArmor = true;

	[JsonProperty]
	[GameStateIgnore]
	public List<MechanicActionBarSlot> Slots = new List<MechanicActionBarSlot>();

	[JsonProperty]
	[GameStateIgnore]
	private MemorizedAbilitiesContainer m_AlreadyAutomaticallyAdded = new MemorizedAbilitiesContainer();

	[JsonProperty]
	[GameStateIgnore]
	private MemorizedAbilitiesContainer m_RemovedFromActionBar = new MemorizedAbilitiesContainer();

	[JsonProperty]
	[GameStateIgnore]
	public Dictionary<string, int> PreferredAbilitiesPositions = new Dictionary<string, int>();

	[JsonProperty]
	private int m_SlotRowIndexConsole;

	[JsonProperty]
	[CanBeNull]
	private BlueprintPortrait m_Portrait;

	[JsonProperty]
	[CanBeNull]
	private PortraitData m_CustomPortrait;

	public bool ShowHelm
	{
		get
		{
			return m_ShowHelm;
		}
		set
		{
			if (m_ShowHelm != value)
			{
				m_ShowHelm = value;
				this.OnChangedHelmetVisibility?.Invoke();
			}
		}
	}

	public bool ShowHelmAboveAll
	{
		get
		{
			return m_ShowHelmAboveAll;
		}
		set
		{
			if (m_ShowHelmAboveAll != value)
			{
				m_ShowHelmAboveAll = value;
				this.OnChangedHelmetVisibilityAboveAll?.Invoke();
			}
		}
	}

	public bool ShowBackpack
	{
		get
		{
			return m_ShowBackpack;
		}
		set
		{
			if (m_ShowBackpack != value)
			{
				m_ShowBackpack = value;
				this.OnChangedBackpackVisibility?.Invoke();
			}
		}
	}

	public bool ShowGloves
	{
		get
		{
			return m_ShowGloves;
		}
		set
		{
			if (m_ShowGloves != value)
			{
				m_ShowGloves = value;
				this.OnChangedGlovesVisibility?.Invoke();
			}
		}
	}

	public bool ShowBoots
	{
		get
		{
			return m_ShowBoots;
		}
		set
		{
			if (m_ShowBoots != value)
			{
				m_ShowBoots = value;
				this.OnChangedBootsVisibility?.Invoke();
			}
		}
	}

	public bool ShowArmor
	{
		get
		{
			return m_ShowArmor;
		}
		set
		{
			if (m_ShowArmor != value)
			{
				m_ShowArmor = value;
				this.OnChangedArmorVisibility?.Invoke();
			}
		}
	}

	public int SlotRowIndexConsole
	{
		get
		{
			return m_SlotRowIndexConsole;
		}
		set
		{
			m_SlotRowIndexConsole = value;
		}
	}

	[CanBeNull]
	public BlueprintPortrait PortraitBlueprint
	{
		get
		{
			if (m_CustomPortrait == null)
			{
				if (!m_Portrait)
				{
					return base.Owner.Blueprint.PortraitSafe;
				}
				return m_Portrait;
			}
			return null;
		}
	}

	[CanBeNull]
	public PortraitData CustomPortraitRaw => m_CustomPortrait;

	[CanBeNull]
	public BlueprintPortrait PortraitBlueprintRaw => m_Portrait;

	public PortraitData Portrait
	{
		get
		{
			if (m_CustomPortrait != null)
			{
				return m_CustomPortrait;
			}
			BlueprintPortrait p = m_Portrait ?? base.Owner.Blueprint.PortraitSafe;
			if (!(Game.Instance.Player.MainCharacter == base.Owner) && Game.Instance.Player.AllCharacters.HasItem((BaseUnitEntity u) => u.UISettings.m_Portrait == p) && p.BackupPortrait != null)
			{
				return p.BackupPortrait.Data;
			}
			return p.Data;
		}
	}

	private event Action OnChangedBackpackVisibility;

	private event Action OnChangedHelmetVisibility;

	private event Action OnChangedHelmetVisibilityAboveAll;

	private event Action OnChangedGlovesVisibility;

	private event Action OnChangedBootsVisibility;

	private event Action OnChangedArmorVisibility;

	public void SetPortrait(BlueprintPortrait portrait)
	{
		if (portrait == BlueprintRoot.Instance.CharGenRoot.CustomPortrait || portrait.Data.IsCustom)
		{
			m_CustomPortrait = portrait.Data;
			m_Portrait = null;
		}
		else
		{
			m_Portrait = portrait;
			m_CustomPortrait = null;
		}
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
		{
			h.HandlePortraitChanged();
		}, isCheckRuntime: true);
	}

	public void SetPortrait(PortraitData portraitData, bool raiseEvent = true)
	{
		m_CustomPortrait = portraitData;
		m_Portrait = null;
		if (raiseEvent)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
			{
				h.HandlePortraitChanged();
			}, isCheckRuntime: true);
		}
	}

	public void SetPortraitUnsafe([CanBeNull] BlueprintPortrait portrait, [CanBeNull] PortraitData portraitData)
	{
		m_CustomPortrait = portraitData;
		m_Portrait = portrait;
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitPortraitChangedHandler>)delegate(IUnitPortraitChangedHandler h)
		{
			h.HandlePortraitChanged();
		}, isCheckRuntime: true);
	}

	public MechanicActionBarSlot GetSlot(int index, BaseUnitEntity unit)
	{
		EnsureSlotsTillIndex(index);
		MechanicActionBarSlot mechanicActionBarSlot = Slots[index];
		if (mechanicActionBarSlot != null && !mechanicActionBarSlot.IsBad())
		{
			return mechanicActionBarSlot;
		}
		mechanicActionBarSlot = new MechanicActionBarSlotEmpty
		{
			Unit = unit
		};
		Slots[index] = mechanicActionBarSlot;
		return mechanicActionBarSlot;
	}

	private void EnsureSlotsTillIndex(int index)
	{
		for (int i = Slots.Count; i < index + 1; i++)
		{
			Slots.Add(new MechanicActionBarSlotEmpty
			{
				Unit = base.Owner
			});
		}
	}

	public void SetSlot(MechanicActionBarSlot slot, int index)
	{
		if (index == -1)
		{
			return;
		}
		if (slot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility)
		{
			m_AlreadyAutomaticallyAdded.Add(mechanicActionBarSlotAbility.Ability.Blueprint, mechanicActionBarSlotAbility.Ability.SourceItem);
			if (m_RemovedFromActionBar.Contains(mechanicActionBarSlotAbility.Ability.Blueprint, mechanicActionBarSlotAbility.Ability.SourceItem))
			{
				m_RemovedFromActionBar.Remove(mechanicActionBarSlotAbility.Ability.Blueprint, mechanicActionBarSlotAbility.Ability.SourceItem);
			}
		}
		EnsureSlotsTillIndex(index);
		Slots[index] = slot;
		if (slot.KeyName != null)
		{
			PreferredAbilitiesPositions[slot.KeyName] = index;
		}
	}

	public void SetSlot(BaseUnitEntity abilityOwner, Ability ability, int index)
	{
		SetSlot(new AbilityWrapper(ability).CreateSlot(abilityOwner), index);
	}

	private bool TrySetSlotInternal(MechanicActionBarSlot slot)
	{
		int i = 0;
		if (PreferredAbilitiesPositions.TryGetValue(slot.KeyName, out i) && GetSlot(i, slot.Unit) is MechanicActionBarSlotEmpty)
		{
			SetSlot(slot, i);
			return true;
		}
		for (; i < 100; i++)
		{
			if (GetSlot(i, slot.Unit) is MechanicActionBarSlotEmpty)
			{
				SetSlot(slot, i);
				return true;
			}
		}
		return false;
	}

	public void CleanupSlots()
	{
		for (int i = 0; i < Slots.Count; i++)
		{
			Slots[i] = new MechanicActionBarSlotEmpty
			{
				Unit = base.Owner
			};
		}
	}

	public void RemoveSlot(int index)
	{
		if (index >= 0 && index < Slots.Count)
		{
			RemoveAlreadyAutomaticallyAddedIfNeed(Slots[index]);
			if (Slots[index].KeyName != null)
			{
				PreferredAbilitiesPositions.Remove(Slots[index].KeyName, out var _);
			}
			MechanicActionBarSlotAbility slotAbility = Slots[index] as MechanicActionBarSlotAbility;
			Slots[index] = new MechanicActionBarSlotEmpty
			{
				Unit = base.Owner
			};
			if (slotAbility != null && !Slots.Any((MechanicActionBarSlot s) => s.KeyName == slotAbility.KeyName))
			{
				m_RemovedFromActionBar.Add(slotAbility.Ability.Blueprint, slotAbility.Ability.SourceItem);
			}
		}
	}

	public void RemoveFromIndexToEnd(int index)
	{
		if (index >= 0 && index < Slots.Count)
		{
			for (int i = index; i < Slots.Count; i++)
			{
				RemoveAlreadyAutomaticallyAddedIfNeed(Slots[i]);
			}
			Slots.RemoveRange(index, Slots.Count - index);
		}
	}

	private void RemoveAlreadyAutomaticallyAddedIfNeed(MechanicActionBarSlot slot)
	{
		if (slot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && mechanicActionBarSlotAbility.Ability != null && m_AlreadyAutomaticallyAdded.Contains(mechanicActionBarSlotAbility.Ability.Blueprint, mechanicActionBarSlotAbility.Ability.SourceItem))
		{
			m_AlreadyAutomaticallyAdded.Remove(mechanicActionBarSlotAbility.Ability.Blueprint, mechanicActionBarSlotAbility.Ability.SourceItem);
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		base.OnApplyPostLoadFixes();
		if (m_AlreadyAutomaticallyAdded.Count() <= 0)
		{
			return;
		}
		HashSet<MemorizedAbilitiesContainer.MemorizedAbilityData> hashSet = new HashSet<MemorizedAbilitiesContainer.MemorizedAbilityData>();
		foreach (MechanicActionBarSlot slot in Slots)
		{
			if (slot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && !(mechanicActionBarSlotAbility.Ability == null))
			{
				hashSet.Add(new MemorizedAbilitiesContainer.MemorizedAbilityData
				{
					ability = mechanicActionBarSlotAbility.Ability.Blueprint,
					sourceItem = mechanicActionBarSlotAbility.Ability.SourceItem
				});
			}
		}
		m_AlreadyAutomaticallyAdded.IntersectWith(hashSet);
	}

	public void TryToInitialize()
	{
		if (!SettingsRoot.Game.Main.AutofillActionbarSlots)
		{
			return;
		}
		List<AbilityWrapper> list = TempList.Get<AbilityWrapper>();
		CollectNewAbilities(base.Owner, list, isMomentum: false);
		list.Sort((AbilityWrapper f1, AbilityWrapper f2) => CompareFactsPriority(f1.Blueprint, f2.Blueprint));
		using (base.Owner.Context.GetDataScope())
		{
			foreach (AbilityWrapper item in list)
			{
				ActionPanelLogic component = item.Blueprint.GetComponent<ActionPanelLogic>();
				if ((component != null && !component.AutoFillConditions.Check()) || m_AlreadyAutomaticallyAdded.Contains(item.Blueprint, item.SourceItem))
				{
					continue;
				}
				Ability ability = item.Ability;
				if (ability == null || !ability.Data.SourceItemIsWeapon)
				{
					if (!TrySetSlotInternal(item.CreateSlot(base.Owner)))
					{
						break;
					}
					Ability ability2 = item.Ability;
					if (ability2 == null || !ability2.Data.SourceItemIsWeapon)
					{
						m_AlreadyAutomaticallyAdded.Add(item.Blueprint, item.SourceItem);
					}
				}
			}
			list.Reverse();
			foreach (AbilityWrapper item2 in list)
			{
				if (!IsWeaponAbilityAndAlreadyAdded(item2))
				{
					Ability ability3 = item2.Ability;
					if (ability3 != null && ability3.Data.SourceItemIsWeapon)
					{
						Slots.Insert(0, item2.CreateSlot(base.Owner));
					}
				}
			}
		}
	}

	private bool IsWeaponAbilityAndAlreadyAdded(AbilityWrapper newAbility)
	{
		Ability ability = newAbility.Ability;
		if (ability != null && ability.Data.SourceItemIsWeapon)
		{
			return Slots.Any(delegate(MechanicActionBarSlot slot)
			{
				if (slot is MechanicActionBarSlotMemorizedSpell mechanicActionBarSlotMemorizedSpell)
				{
					return newAbility.SpellSlot == mechanicActionBarSlotMemorizedSpell.SpellSlot;
				}
				if (slot is MechanicActionBarSlotSpontaneousSpell mechanicActionBarSlotSpontaneousSpell)
				{
					return newAbility.SpontaneousSpell == mechanicActionBarSlotSpontaneousSpell.Spell;
				}
				if (slot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility)
				{
					return newAbility.Ability?.Data == mechanicActionBarSlotAbility.Ability;
				}
				return slot is MechanicActionBarSlotActivableAbility mechanicActionBarSlotActivableAbility && newAbility.ActivatableAbility == mechanicActionBarSlotActivableAbility.ActivatableAbility;
			});
		}
		return false;
	}

	public List<MechanicActionBarSlot> GetMomentumSlots()
	{
		List<AbilityWrapper> list = TempList.Get<AbilityWrapper>();
		CollectNewAbilities(base.Owner, list, isMomentum: true);
		return list.Select((AbilityWrapper a) => a.CreateSlot(base.Owner)).ToList();
	}

	private void CollectNewAbilities(BaseUnitEntity unit, List<AbilityWrapper> results, bool isMomentum)
	{
		foreach (Ability item in unit.Abilities.Visible)
		{
			IItemEntity sourceItem = item.SourceItem;
			if (!(sourceItem is ItemEntityWeapon) && !(sourceItem is ItemEntityShield) && item.Blueprint.IsMomentum == isMomentum)
			{
				ActionPanelLogic component = item.Blueprint.GetComponent<ActionPanelLogic>();
				if ((isMomentum || component == null || component.AutoFillConditions.Check()) && (isMomentum || !m_AlreadyAutomaticallyAdded.Contains(item.Blueprint, (ItemEntity)item.SourceItem)) && (isMomentum || !m_RemovedFromActionBar.Contains(item.Blueprint, (ItemEntity)item.SourceItem)))
				{
					results.Add(new AbilityWrapper(item));
				}
			}
		}
	}

	public void RemoveSlot(Ability ability)
	{
		if (Slots == null)
		{
			return;
		}
		for (int i = 0; i < Slots.Count; i++)
		{
			if (Slots[i] is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && mechanicActionBarSlotAbility.IsSameAbility(ability))
			{
				Slots[i] = new MechanicActionBarSlotEmpty
				{
					Unit = mechanicActionBarSlotAbility.Unit
				};
				m_AlreadyAutomaticallyAdded.Remove(ability.Data.Blueprint, ability.Data.SourceItem);
			}
		}
	}

	public void SubscribeOnBackpackVisibilityChange(Action subscriber)
	{
		OnChangedBackpackVisibility += subscriber;
	}

	public void SubscribeOnHelmetVisibilityChange(Action subscriber)
	{
		OnChangedHelmetVisibility += subscriber;
	}

	public void SubscribeOnHelmetVisibilityAboveAllChange(Action subscriber)
	{
		OnChangedHelmetVisibilityAboveAll += subscriber;
	}

	public void SubscribeOnGlovesVisibilityChange(Action subscriber)
	{
		OnChangedGlovesVisibility += subscriber;
	}

	public void SubscribeOnBootsVisibilityChange(Action subscriber)
	{
		OnChangedBootsVisibility += subscriber;
	}

	public void SubscribeOnArmorVisibilityChange(Action subscriber)
	{
		OnChangedArmorVisibility += subscriber;
	}

	public void UnsubscribeFromBackpackVisibilityChange(Action subscriber)
	{
		OnChangedBackpackVisibility -= subscriber;
	}

	public void UnsubscribeFromHelmetVisibilityChange(Action subscriber)
	{
		OnChangedHelmetVisibility -= subscriber;
	}

	public void UnsubscribeFromHelmetVisibilityAboveAllChange(Action subscriber)
	{
		OnChangedHelmetVisibilityAboveAll -= subscriber;
	}

	public void UnsubscribeFromGlovesVisibilityChange(Action subscriber)
	{
		OnChangedGlovesVisibility -= subscriber;
	}

	public void UnsubscribeFromBootsVisibilityChange(Action subscriber)
	{
		OnChangedBootsVisibility -= subscriber;
	}

	public void UnsubscribeFromArmorVisibilityChange(Action subscriber)
	{
		OnChangedArmorVisibility -= subscriber;
	}

	private static int CompareFactsPriority(BlueprintUnitFact f1, BlueprintUnitFact f2)
	{
		int num = BlueprintComponentExtendAsObject.Or(f1.GetComponent<ActionPanelLogic>(), null)?.Priority ?? 0;
		int value = BlueprintComponentExtendAsObject.Or(f2.GetComponent<ActionPanelLogic>(), null)?.Priority ?? 0;
		return -num.CompareTo(value);
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_ShowHelm);
		result.Append(ref m_ShowHelmAboveAll);
		result.Append(ref m_ShowBackpack);
		result.Append(ref m_ShowGloves);
		result.Append(ref m_ShowBoots);
		result.Append(ref m_ShowArmor);
		result.Append(ref m_SlotRowIndexConsole);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_Portrait);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<PortraitData>.GetHash128(m_CustomPortrait);
		result.Append(ref val3);
		return result;
	}
}
