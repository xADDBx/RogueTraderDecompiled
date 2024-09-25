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
	private bool m_ShowHelmAboveAll;

	[JsonProperty]
	private bool m_ShowBackpack = true;

	[JsonProperty]
	[GameStateIgnore]
	public List<MechanicActionBarSlot> Slots = new List<MechanicActionBarSlot>();

	[JsonProperty]
	[GameStateIgnore]
	private MemorizedAbilitiesContainer m_AlreadyAutomaticallyAdded = new MemorizedAbilitiesContainer();

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
		FillSlots(index);
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

	private void FillSlots(int index)
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
		if (index != -1)
		{
			FillSlots(index);
			Slots[index] = slot;
		}
	}

	public void SetSlot(BaseUnitEntity abilityOwner, Ability ability, int index)
	{
		SetSlot(new AbilityWrapper(ability).CreateSlot(abilityOwner), index);
	}

	private bool TrySetSlotInternal(MechanicActionBarSlot slot)
	{
		for (int i = 0; i < 100; i++)
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
		if (index < Slots.Count)
		{
			Slots[index] = new MechanicActionBarSlotEmpty
			{
				Unit = base.Owner
			};
		}
	}

	public void RemoveFromIndexToEnd(int index)
	{
		Slots.RemoveRange(index, Slots.Count - index);
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
			if (!(item.SourceItem is ItemEntityWeapon) && item.Blueprint.IsMomentum == isMomentum)
			{
				ActionPanelLogic component = item.Blueprint.GetComponent<ActionPanelLogic>();
				if ((isMomentum || component == null || component.AutoFillConditions.Check()) && (isMomentum || !m_AlreadyAutomaticallyAdded.Contains(item.Blueprint, (ItemEntity)item.SourceItem)))
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
		result.Append(ref m_SlotRowIndexConsole);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_Portrait);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<PortraitData>.GetHash128(m_CustomPortrait);
		result.Append(ref val3);
		return result;
	}
}
