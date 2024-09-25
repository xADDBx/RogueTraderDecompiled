using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Photon.Realtime;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar;

public class ActionBarSlotVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IHoverActionBarSlotHandler, ISubscriber, IAbilityTargetSelectionUIHandler, INetPingActionBarAbility, INetLobbyPlayersHandler, INetRoleSetHandler, IWeaponSlotHandler
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<Sprite> ForeIcon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<Sprite> MicroAbilityIcon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<int> ResourceCount = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ResourceCost = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ResourceAmount = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ActionPointCost = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> AmmoCost = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> IsCasting = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsPossibleActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsPossibleWithoutNetRole = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsFake = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasConvert = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasAvailableConvert = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsEmpty = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsReload = new ReactiveProperty<bool>();

	public ItemEntityWeapon Weapon;

	public readonly ReactiveProperty<int> CurrentAmmo = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> MaxAmmo = new ReactiveProperty<int>();

	public int MaxWeaponAbilityAmmo;

	public readonly ReactiveProperty<bool> IsOnCooldown = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> CooldownText = new ReactiveProperty<string>();

	public readonly ReactiveCommand<bool> OnClickCommand = new ReactiveCommand<bool>();

	public readonly ReactiveProperty<ActionBarConvertedVM> ConvertedVm = new ReactiveProperty<ActionBarConvertedVM>(null);

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<bool> IsAlerted = new ReactiveProperty<bool>(initialValue: false);

	private List<AbilityData> m_Conversion;

	private bool m_TempTooltipState;

	private bool m_HasAttackAbilityGroup;

	private bool m_TargetSelectionStarted;

	public readonly ReactiveCommand<(NetPlayer player, bool show)> CoopPingActionBarSlot = new ReactiveCommand<(NetPlayer, bool)>();

	public readonly ReactiveProperty<bool> IsSelectionBusy = new ReactiveProperty<bool>();

	public readonly bool IsInCharScreen;

	public readonly BoolReactiveProperty MoveAbilityMode;

	public readonly WeaponSlotType WeaponSlotType = WeaponSlotType.None;

	public readonly ReactiveCommand UpdateDragAndDropState = new ReactiveCommand();

	private Sprite m_OriginIcon;

	public MechanicActionBarSlot MechanicActionBarSlot { get; private set; }

	public int Index { get; }

	public AbilityData AbilityData
	{
		get
		{
			if (MechanicActionBarSlot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility)
			{
				return mechanicActionBarSlotAbility.Ability;
			}
			if (MechanicActionBarSlot is MechanicActionBarSlotItem mechanicActionBarSlotItem)
			{
				return mechanicActionBarSlotItem.Ability?.Data;
			}
			return null;
		}
	}

	public bool IsHeroicAct => (AbilityData?.Blueprint?.IsHeroicAct).GetValueOrDefault();

	public bool IsDesperateMeasure
	{
		get
		{
			if ((AbilityData?.Blueprint?.IsMomentum).GetValueOrDefault())
			{
				return !IsHeroicAct;
			}
			return false;
		}
	}

	public ActionBarSlotVM(MechanicActionBarSlot abs, int index = -1, bool isInCharScreen = false, BoolReactiveProperty moveMode = null, WeaponSlotType weaponSlotType = WeaponSlotType.None)
	{
		Index = index;
		IsInCharScreen = isInCharScreen;
		MoveAbilityMode = moveMode;
		WeaponSlotType = weaponSlotType;
		AddDisposable(EventBus.Subscribe(this));
		SetMechanicSlot(abs);
	}

	public ActionBarSlotVM(WeaponAbility abilityFact, ItemEntityWeapon weapon)
	{
		IsFake.Value = true;
		ReactiveProperty<Sprite> icon = Icon;
		BlueprintAbility ability = abilityFact.Ability;
		icon.Value = ((ability != null) ? ObjectExtensions.Or(ability.Icon, UIConfig.Instance.UIIcons.DefaultAbilityIcon) : null);
		ResourceCount.Value = -1;
		ActionPointCost.Value = abilityFact.AP;
		Tooltip.Value = new TooltipTemplateDataProvider(abilityFact.Ability);
		IsReload.Value = UIUtilityItem.IsReload(abilityFact.Ability);
		Weapon = weapon;
		MaxWeaponAbilityAmmo = (IsReload.Value ? UIUtilityItem.GetMaxAbilityAmmo(Weapon) : 0);
		UpdateReloadAmmo();
	}

	protected override void DisposeImplementation()
	{
		MechanicActionBarSlot mechanicActionBarSlot = MechanicActionBarSlot;
		if (mechanicActionBarSlot != null && mechanicActionBarSlot.HoverState)
		{
			MechanicActionBarSlot.OnHover(state: false);
		}
		CloseConvert();
	}

	public void SetMechanicSlot(MechanicActionBarSlot abs)
	{
		if (abs != MechanicActionBarSlot)
		{
			MechanicActionBarSlot mechanicActionBarSlot = MechanicActionBarSlot;
			if (mechanicActionBarSlot != null && mechanicActionBarSlot.HoverState)
			{
				MechanicActionBarSlot.OnHover(state: false);
			}
			IsEmpty.Value = abs is MechanicActionBarSlotEmpty;
			MechanicActionBarSlot = abs;
			m_Conversion = (from a in MechanicActionBarSlot.GetConvertedAbilityData()
				where a.IsVisible()
				select a).ToList();
			Icon.Value = MechanicActionBarSlot.GetIcon() ?? UIConfig.Instance.UIIcons.DefaultAbilityIcon;
			ForeIcon.Value = MechanicActionBarSlot.GetForeIcon();
			HasConvert.Value = m_Conversion.Count > 0;
			Tooltip.Value = MechanicActionBarSlot.GetTooltipTemplate();
			m_HasAttackAbilityGroup = MechanicActionBarSlot.HasWeaponAbilityGroup();
			MechanicActionBarSlotAbility mechanicActionBarSlotAbility = abs as MechanicActionBarSlotAbility;
			IsReload.Value = UIUtilityItem.IsReload(mechanicActionBarSlotAbility?.Ability);
			Weapon = mechanicActionBarSlotAbility?.Ability?.SourceItem as ItemEntityWeapon;
			MaxWeaponAbilityAmmo = (IsReload.Value ? UIUtilityItem.GetMaxAbilityAmmo(Weapon) : 0);
			UpdateResources();
			DelayedInvoker.InvokeInTime(UpdateResources, 0.5f);
		}
	}

	public void UpdateResources()
	{
		UpdateReloadAmmo();
		if (MechanicActionBarSlot != null && MechanicActionBarSlot.Unit != null)
		{
			MechanicActionBarSlot.UpdateResourceCount();
			MechanicActionBarSlot.UpdateResourceCost();
			MechanicActionBarSlot.UpdateResourceAmount();
			ResourceCount.Value = MechanicActionBarSlot.GetResource();
			ResourceCost.Value = MechanicActionBarSlot.GetResourceCost();
			ResourceAmount.Value = MechanicActionBarSlot.GetResourceAmount();
			AmmoCost.Value = MechanicActionBarSlot.AmmoCost();
			IsCasting.Value = MechanicActionBarSlot.IsCasting();
			IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive;
			IsPossibleWithoutNetRole.Value = MechanicActionBarSlot.IsPossibleActiveWithoutNetRole;
			ActionPointCost.Value = MechanicActionBarSlot.ActionPointCost();
			HasAvailableConvert.Value = m_Conversion.Any((AbilityData abilityData) => abilityData.IsAvailable);
			if (AbilityData != null)
			{
				IsSelected.Value = Game.Instance.SelectedAbilityHandler != null && Game.Instance.SelectedAbilityHandler.Ability == AbilityData;
				IsOnCooldown.Value = AbilityData.IsOnCooldown;
				CooldownText.Value = ((AbilityData.Cooldown > 0) ? AbilityData.Cooldown.ToString() : string.Empty);
			}
		}
	}

	public void OnMainClick()
	{
		if (MechanicActionBarSlot == null)
		{
			return;
		}
		if (PhotonManager.NetGame.CurrentState == NetGame.State.Playing && !MechanicActionBarSlot.Unit.IsMyNetRole())
		{
			PhotonManager.Ping.PressPing(delegate
			{
				if (!string.IsNullOrWhiteSpace(MechanicActionBarSlot.KeyName))
				{
					PhotonManager.Ping.PingActionBarAbility(MechanicActionBarSlot.KeyName, MechanicActionBarSlot.Unit, Index, WeaponSlotType);
				}
			});
			return;
		}
		MechanicActionBarSlot.PlaySound();
		MechanicActionBarSlot mechanicActionBarSlot = MechanicActionBarSlot;
		if (!(mechanicActionBarSlot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility))
		{
			if (!(mechanicActionBarSlot is MechanicActionBarSlotMemorizedSpell mechanicActionBarSlotMemorizedSpell))
			{
				if (mechanicActionBarSlot is MechanicActionBarSlotSpontaneousSpell mechanicActionBarSlotSpontaneousSpell && m_Conversion.Any() && mechanicActionBarSlotSpontaneousSpell.GetResource() > 0 && !mechanicActionBarSlotSpontaneousSpell.IsPossibleActive)
				{
					OnShowConvertRequest();
					return;
				}
			}
			else if (m_Conversion.Any() && !mechanicActionBarSlotMemorizedSpell.IsPossibleActive)
			{
				OnShowConvertRequest();
				return;
			}
		}
		else if (mechanicActionBarSlotAbility.Ability.Blueprint.HasVariants && mechanicActionBarSlotAbility.IsPossibleActive)
		{
			OnShowConvertRequest();
			return;
		}
		if (MechanicActionBarSlot.IsPossibleActive)
		{
			OnClickCommand.Execute(parameter: false);
		}
		MechanicActionBarSlot.OnClick();
	}

	public void OnSupportClick()
	{
		MechanicActionBarSlot?.OnAutoUseToggle();
		OnClickCommand.Execute(parameter: true);
	}

	public void OnHoverOn()
	{
		MechanicActionBarSlot?.OnHover(state: true);
		if (m_HasAttackAbilityGroup)
		{
			EventBus.RaiseEvent(delegate(IHoverActionBarSlotHandler h)
			{
				h.HandlePointerEnterAttackGroupAbilitySlot(MechanicActionBarSlot);
			});
		}
		EventBus.RaiseEvent(delegate(IHoverActionBarSlotHandler h)
		{
			h.HandlePointerEnterActionBarSlot(MechanicActionBarSlot);
		});
	}

	public void OnHoverOff()
	{
		MechanicActionBarSlot?.OnHover(state: false);
		if (m_HasAttackAbilityGroup)
		{
			EventBus.RaiseEvent(delegate(IHoverActionBarSlotHandler h)
			{
				h.HandlePointerExitAttackGroupAbilitySlot(MechanicActionBarSlot);
			});
		}
		EventBus.RaiseEvent(delegate(IHoverActionBarSlotHandler h)
		{
			h.HandlePointerExitActionBarSlot(MechanicActionBarSlot);
		});
	}

	public void OnShowConvertRequest()
	{
		HandleConvertRequest(CreateConvertSlot);
		MechanicActionBarSlotSpontaneusConvertedSpell CreateConvertSlot(AbilityData data)
		{
			return new MechanicActionBarSlotSpontaneusConvertedSpell
			{
				Spell = data,
				Unit = MechanicActionBarSlot.Unit
			};
		}
	}

	public void OnShowVariantsConvertRequest(MechanicActionBarShipWeaponSlot variantsShipWeaponSlot)
	{
		HandleConvertRequest(CreateArsenalSlot);
		MechanicActionBarArsenalSlot CreateArsenalSlot(AbilityData data)
		{
			return new MechanicActionBarArsenalSlot(variantsShipWeaponSlot.WeaponSlot, CloseConvert)
			{
				Spell = data,
				Unit = variantsShipWeaponSlot.Unit
			};
		}
	}

	private void HandleConvertRequest(Func<AbilityData, MechanicActionBarSlotSpontaneusConvertedSpell> createSlot)
	{
		if (ConvertedVm.Value == null || ConvertedVm.Value.IsDisposed)
		{
			if (m_Conversion.Count != 0)
			{
				ConvertedVm.Value = new ActionBarConvertedVM(m_Conversion.Select(createSlot).ToList(), CloseConvert);
				Tooltip.Value = null;
			}
		}
		else
		{
			CloseConvert();
			Tooltip.Value = MechanicActionBarSlot.GetTooltipTemplate();
		}
	}

	private void CloseConvert()
	{
		ConvertedVm.Value?.Dispose();
		ConvertedVm.Value = null;
	}

	public void CloseConvertsOnTurnStart()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			CloseConvert();
		}
	}

	public void HandlePointerEnterActionBarSlot(MechanicActionBarSlot ability)
	{
	}

	public void HandlePointerExitActionBarSlot(MechanicActionBarSlot ability)
	{
	}

	public void HandlePointerEnterAttackGroupAbilitySlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted && ability != MechanicActionBarSlot)
		{
			TryTurnAlertOn();
		}
	}

	public void HandlePointerExitAttackGroupAbilitySlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted)
		{
			IsAlerted.Value = false;
		}
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		if (ability == AbilityData)
		{
			IsSelected.Value = true;
		}
		if (CheckAbilityHasAttackAbilityGroupCooldown(ability.Blueprint) && (!(MechanicActionBarSlot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility) || !(mechanicActionBarSlotAbility.Ability == ability)))
		{
			m_TargetSelectionStarted = true;
			TryTurnAlertOn();
		}
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		IsSelected.Value = false;
		m_TargetSelectionStarted = false;
		IsAlerted.Value = false;
	}

	private void TryTurnAlertOn()
	{
		if (Game.Instance.TurnController.TurnBasedModeActive && m_HasAttackAbilityGroup)
		{
			IsAlerted.Value = true;
		}
	}

	private bool CheckAbilityHasAttackAbilityGroupCooldown(BlueprintAbility blueprintAbility)
	{
		foreach (BlueprintAbilityGroup abilityGroup in blueprintAbility.AbilityGroups)
		{
			if (abilityGroup.NameSafe() == "WeaponAttackAbilityGroup")
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateReloadAmmo()
	{
		if (!IsReload.Value || Weapon == null)
		{
			CurrentAmmo.Value = 0;
			MaxAmmo.Value = 0;
		}
		else
		{
			CurrentAmmo.Value = Weapon.CurrentAmmo;
			MaxAmmo.Value = Weapon.Blueprint.WarhammerMaxAmmo;
		}
	}

	public void ChooseAbility()
	{
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.ChooseAbilityToSlot(Index);
		});
	}

	public void ClearSlot()
	{
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.DeleteSlot(Index);
		});
	}

	public void MoveAbility()
	{
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.SetMoveAbilityMode(on: true);
		});
	}

	public void HandlePingActionBarAbility(NetPlayer player, string keyName, Entity characterEntityRef, int slotIndex, WeaponSlotType weaponSlotType)
	{
		if (!string.IsNullOrWhiteSpace(keyName) && characterEntityRef == MechanicActionBarSlot.Unit && slotIndex == Index && WeaponSlotType == weaponSlotType)
		{
			CoopPingActionBarSlot.Execute((player, keyName == MechanicActionBarSlot.KeyName));
		}
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
		IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive;
		IsPossibleWithoutNetRole.Value = MechanicActionBarSlot.IsPossibleActiveWithoutNetRole;
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive;
		IsPossibleWithoutNetRole.Value = MechanicActionBarSlot.IsPossibleActiveWithoutNetRole;
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}

	public void HandleRoleSet(string entityId)
	{
		IsPossibleActive.Value = MechanicActionBarSlot.IsPossibleActive;
		IsPossibleWithoutNetRole.Value = MechanicActionBarSlot.IsPossibleActiveWithoutNetRole;
		UpdateDragAndDropState.Execute();
	}

	public void OverrideIcon(Sprite icon)
	{
		if (icon == null)
		{
			Icon.Value = m_OriginIcon;
			m_OriginIcon = null;
			return;
		}
		if ((object)m_OriginIcon == null)
		{
			m_OriginIcon = Icon.Value;
		}
		Icon.Value = icon;
	}

	void IWeaponSlotHandler.HandleActiveWeaponIndexChanged(WeaponSlot weaponSlot)
	{
		if (MechanicActionBarSlot is MechanicActionBarShipWeaponSlot mechanicActionBarShipWeaponSlot && mechanicActionBarShipWeaponSlot.WeaponSlot == weaponSlot)
		{
			BaseUnitEntity unit = MechanicActionBarSlot.Unit;
			MechanicActionBarShipWeaponSlot mechanicSlot = new MechanicActionBarShipWeaponSlot(weaponSlot, unit)
			{
				Ability = weaponSlot.ActiveAbility.Data
			};
			SetMechanicSlot(mechanicSlot);
		}
	}
}
