using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Photon.Realtime;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Party;

public class PartyCharacterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, ILevelUpCompleteUIHandler, IPartyCombatHandler, ILevelUpManagerUIHandler, ICanAccessServiceWindowsHandler, IPartyEncumbranceHandler, IUnitEncumbranceHandler, INetRoleSetHandler, INetStopPlayingHandler, INetLobbyPlayersHandler
{
	public readonly ReactiveProperty<bool> IsEnable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsSingleSelected = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsLinked = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsLevelUp = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsLevelUpCurrent = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsLevelUpInProgress = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsServiceWindowAvailable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsInCombat = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<Sprite> NetAvatar = new ReactiveProperty<Sprite>(null);

	public readonly ReactiveProperty<Sprite> SelectorFrameIcon = new ReactiveProperty<Sprite>(null);

	public readonly ReactiveProperty<string> CharacterName = new ReactiveProperty<string>(null);

	public readonly ReactiveProperty<bool> HasPet = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsHudActive = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> AscendedLabel = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<Sprite> PetMasterNumberIcon = new ReactiveProperty<Sprite>(null);

	public readonly ReactiveProperty<Encumbrance> CharacterEncumbrance = new ReactiveProperty<Encumbrance>();

	public readonly ReactiveProperty<Encumbrance> PartyEncumbrance = new ReactiveProperty<Encumbrance>();

	public readonly ReactiveCommand<bool> FakeSelected = new ReactiveCommand<bool>();

	public readonly UnitHealthPartVM HealthPartVM;

	public readonly UnitPortraitPartVM PortraitPartVM;

	public readonly UnitBuffPartVM BuffPartVM;

	public readonly UnitBarkPartVM BarkPartVM;

	private PartyCharacterVM m_Pet;

	private PartyCharacterVM m_Master;

	private readonly Dictionary<BaseUnitEntity, int> m_CharacterLevel = new Dictionary<BaseUnitEntity, int>();

	private readonly ReactiveCommand m_UpgradeLevel = new ReactiveCommand();

	private readonly ReactiveCommand m_IsNewLevel = new ReactiveCommand();

	private readonly ReactiveProperty<BaseUnitEntity> m_Unit = new ReactiveProperty<BaseUnitEntity>(null);

	private readonly Action<bool> m_NextPrevAction;

	public readonly int Index;

	private BlueprintBuff AscendBuff;

	public bool CanSwitch
	{
		get
		{
			if (UnitEntityData != null)
			{
				return Game.Instance.SelectionCharacter.ReorderEnable;
			}
			return false;
		}
	}

	public BaseUnitEntity UnitEntityData => m_Unit.Value;

	private SelectionCharacterController SelectionCharacter => Game.Instance.SelectionCharacter;

	public PartyCharacterVM(Action<bool> nextPrevAction, int index)
	{
		m_NextPrevAction = nextPrevAction;
		Index = index;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(HealthPartVM = new UnitHealthPartVM(m_Unit));
		AddDisposable(PortraitPartVM = new UnitPortraitPartVM());
		AddDisposable(BuffPartVM = new UnitBuffPartVM(m_Unit.Value));
		AddDisposable(BarkPartVM = new UnitBarkPartVM());
		HasPet.Value = false;
		IsHudActive.Value = true;
		AscendBuff = BlueprintRoot.Instance.StatusBuffs.AscendBuff.Get();
		AddDisposable(m_Unit.Subscribe(delegate(BaseUnitEntity value)
		{
			PortraitPartVM.SetUnitData(value);
			BuffPartVM.SetUnitData(value);
			BarkPartVM.SetUnitData(value);
			UpdateLevelUpField(value);
			UpdateEncumbranceField(value);
			SelectorFrameIcon.Value = SetFrameColor(value);
			IsEnable.Value = value != null;
			IsLinked.Value = value?.IsLink ?? false;
			if (value != null && (value.GetOptional<UnitPartPetOwner>() != null || value.IsPet))
			{
				HasPet.Value = true;
			}
			CharacterName.Value = value?.CharacterName;
		}));
		AddDisposable(UniRxExtensionMethods.Subscribe(MainThreadDispatcher.FrequentUpdateAsObservable(), delegate
		{
			if (IsEnable.Value)
			{
				IsSelected.Value = SelectionCharacter.IsSelected(UnitEntityData) || UnitEntityData.IsFakeSelected;
				IsHudActive.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.None;
				AscendedLabel.Value = UnitEntityData.Buffs.GetBuff(AscendBuff) != null;
				bool flag = ((SelectionCharacter.SelectedUnitInUI.Value != null) ? (SelectionCharacter.SelectedUnitInUI.Value == UnitEntityData) : (SelectionCharacter.SingleSelectedUnit.Value == UnitEntityData));
				IsSingleSelected.Value = flag || UnitEntityData.IsFakeSelected;
				IsLinked.Value = ((m_Master != null) ? m_Master.IsLinked.Value : UnitEntityData.IsLink);
				if (!PetMasterNumberIcon.Value)
				{
					HasPet.Value = false;
				}
				HandleServiceWindowsBlocked();
			}
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_UpgradeLevel, delegate
		{
			UISounds.Instance.Sounds.Character.LevelUpgradedNotification.Play();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_IsNewLevel, delegate
		{
			UISounds.Instance.Sounds.Character.NewLevelNotification.Play();
		}));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleUnitClick(bool isDoubleClick = false)
	{
		ClickUnitHandler.HandleClickControllableUnit(UnitEntityData, isDoubleClick);
		if (UnitEntityData.IsPet)
		{
			IsSelected.Value = SelectionCharacter.IsSelected(UnitEntityData) || UnitEntityData.IsFakeSelected;
			IsSingleSelected.Value = ((SelectionCharacter.SelectedUnitInUI.Value != null) ? (SelectionCharacter.SelectedUnitInUI.Value == UnitEntityData) : (SelectionCharacter.SingleSelectedUnit.Value == UnitEntityData));
			IsLinked.Value = UnitEntityData.IsLink;
		}
	}

	public void ToggleLinkUnit()
	{
		if (!UnitEntityData.IsPet)
		{
			if (SelectionManagerConsole.Instance.IsLinked(UnitEntityData))
			{
				SelectionManagerConsole.Instance.UnlinkUnit(UnitEntityData);
			}
			else
			{
				SelectionManagerConsole.Instance.LinkUnit(UnitEntityData);
			}
			UpdateLink();
		}
	}

	public void UpdateLink()
	{
		if (!UnitEntityData.IsPet)
		{
			IsLinked.Value = UnitEntityData?.IsLink ?? false;
		}
		if (m_Pet != null)
		{
			m_Pet.IsLinked.Value = UnitEntityData?.IsLink ?? false;
		}
	}

	public void SelectAll()
	{
		UIAccess.SelectionManager.SelectAll();
		if (!UIUtility.IsGlobalMap() && UnitEntityData.IsViewActive)
		{
			CameraRig.Instance.ScrollTo(UnitEntityData.Position);
			if ((bool)SettingsRoot.Controls.CameraFollowsUnit)
			{
				Game.Instance.CameraController?.Follower?.Follow(UnitEntityData);
			}
		}
	}

	public void SetUnitData(BaseUnitEntity unitEntityData)
	{
		m_Unit.Value = unitEntityData;
		if (unitEntityData != null && !unitEntityData.IsDisposed)
		{
			HasPet.Value = false;
			UnitPartPetOwner optional = unitEntityData.GetOptional<UnitPartPetOwner>();
			if (unitEntityData.IsPet || optional != null)
			{
				HasPet.Value = true;
			}
			m_CharacterLevel.TryAdd(unitEntityData, unitEntityData.Progression.CharacterLevel);
			if (PhotonManager.Lobby.IsActive && UnitEntityData != null)
			{
				SetNetAvatar();
			}
		}
	}

	private void UpdateLevelUpField(BaseUnitEntity unit, bool levelUpSound = false)
	{
		if (UnitEntityData == null || unit == null || UnitEntityData != unit || UnitEntityData.IsDisposed || unit.IsDisposed)
		{
			return;
		}
		IsLevelUp.Value = !IsInCombat.Value && UnitEntityData.Progression.CanLevelUp;
		if (levelUpSound && IsLevelUp.Value)
		{
			m_IsNewLevel.Execute();
		}
		if (m_CharacterLevel.TryGetValue(unit, out var value))
		{
			if (unit.Progression.CharacterLevel > value)
			{
				m_UpgradeLevel.Execute();
			}
			m_CharacterLevel[unit] = unit.Progression.CharacterLevel;
		}
	}

	private void UpdateEncumbranceField(BaseUnitEntity unit)
	{
		if (UnitEntityData != null && unit != null && UnitEntityData == unit && !UnitEntityData.IsDisposed && !unit.IsDisposed)
		{
			CharacterEncumbrance.Value = UnitEntityData.EncumbranceData?.Value ?? Encumbrance.Light;
			ReactiveProperty<Encumbrance> partyEncumbrance = PartyEncumbrance;
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			partyEncumbrance.Value = ((loadedAreaState == null || !loadedAreaState.Settings.CapitalPartyMode) ? Game.Instance.Player.Encumbrance : Encumbrance.Light);
		}
	}

	public void LevelUp()
	{
		if (UnitEntityData.CanBeControlled())
		{
			EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
			{
				h.HandleOpenCharacterInfoPage(CharInfoPageType.LevelProgression, UnitEntityData);
			});
		}
	}

	public void HandleUnitGainExperience(int gained, bool withSound = false)
	{
		UpdateLevelUpField(EventInvokerExtensions.BaseUnitEntity, withSound);
	}

	public void HandleLevelUpComplete(bool isChargen)
	{
		UpdateLevelUpField(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleMythicSelectionComplete(BaseUnitEntity unit)
	{
		UpdateLevelUpField(unit);
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		IsInCombat.Value = inCombat;
		UpdateLevelUpField(UnitEntityData);
	}

	public void ChangePartyEncumbrance(Encumbrance prevEncumbrance)
	{
		UpdateEncumbranceField(UnitEntityData);
	}

	public void ChangeUnitEncumbrance(Encumbrance prevEncumbrance)
	{
		UpdateEncumbranceField(UnitEntityData);
	}

	public void NextPrev(bool dir)
	{
		m_NextPrevAction?.Invoke(dir);
	}

	public void OnCharacterHover(bool value)
	{
		EventBus.RaiseEvent((IBaseUnitEntity)UnitEntityData, (Action<IPortraitHoverUIHandler>)delegate(IPortraitHoverUIHandler h)
		{
			h.HandlePortraitHover(value);
		}, isCheckRuntime: true);
		EventBus.RaiseEvent(delegate(IPartyCharacterHoverHandler h)
		{
			h.HandlePartyCharacterHover(UnitEntityData, value);
		});
	}

	public void ShowBark(string text)
	{
		BarkPartVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkPartVM.HideBark();
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
		IsLevelUpCurrent.Value = manager.TargetUnit == UnitEntityData;
		IsLevelUpInProgress.Value = true;
	}

	public void HandleDestroyLevelUpManager()
	{
		IsLevelUpCurrent.Value = false;
		IsLevelUpInProgress.Value = false;
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
		UpdateLevelUpField(UnitEntityData);
	}

	public void HandleUISelectionChanged()
	{
	}

	public void HandleRoleSet(string entityId)
	{
		if (UnitEntityData != null && UnitEntityData.UniqueId.Equals(entityId, StringComparison.Ordinal))
		{
			SetNetAvatar();
		}
	}

	private void SetNetAvatar()
	{
		Sprite value = (UnitEntityData.IsMyNetRole() ? null : UnitEntityData.GetPlayer().GetPlayerIcon());
		NetAvatar.Value = value;
	}

	void INetStopPlayingHandler.HandleStopPlaying()
	{
		NetAvatar.Value = null;
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		SetNetAvatar();
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

	public void HandleServiceWindowsBlocked()
	{
		IsServiceWindowAvailable.Value = !Game.Instance.Player.CharacterInfoWindowBlocked && !Game.Instance.Player.ServiceWindowsBlocked;
	}

	public void ClearPetMasterData()
	{
		SetNumPetMasterLabelNumber(0);
		SetMasterVMReference(null);
		SetPetVMReference(null);
		AscendedLabel.Value = false;
		FakeSelected.Execute(parameter: false);
	}

	public void HandleSelectAsPet(bool value)
	{
		FakeSelected.Execute(value);
	}

	private Sprite SetFrameColor(BaseUnitEntity value)
	{
		if (value != null)
		{
			return UIConfig.Instance.UIIcons.GetSelectorFrame(value.IsPet);
		}
		return null;
	}

	public void SetNumPetMasterLabelNumber(int value)
	{
		PetMasterNumberIcon.Value = UIConfig.Instance.UIIcons.GetPetNumberIcon(value);
	}

	public void SetMasterVMReference(PartyCharacterVM vmReference)
	{
		m_Master = vmReference;
	}

	public void SetPetVMReference(PartyCharacterVM vmReference)
	{
		m_Pet = vmReference;
	}
}
