using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Photon.Realtime;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;

public class SurfaceActionBarVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, IWarhammerAttackHandler, IUnitCommandActHandler, IUnitCommandEndHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IDeliverAbilityEffectHandler, IUnitAbilityCooldownHandler, IAbilityExecutionProcessHandler, ILevelUpCompleteUIHandler, ILevelUpManagerUIHandler, IDialogInteractionHandler, IHoverActionBarSlotHandler, IAbilityTargetSelectionUIHandler, IAreaActivationHandler, IUnitDirectHoverUIHandler, IFullScreenUIHandler, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, INetLobbyPlayersHandler, INetRoleSetHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler, ITurnStartHandler
{
	public readonly SurfaceActionBarPartConsumablesVM Consumables;

	public readonly SurfaceActionBarPartWeaponsVM Weapons;

	public readonly SurfaceActionBarPartAbilitiesVM Abilities;

	public readonly SurfaceMomentumVM SurfaceMomentumVM;

	public readonly VeilThicknessVM VeilThickness;

	public readonly ReactiveProperty<SurfaceCombatUnitVM> CurrentCombatUnit;

	private readonly BoolReactiveProperty m_IsVisible = new BoolReactiveProperty();

	private bool m_IsInGame;

	private bool m_IsInFullScreenUI;

	public readonly ReactiveProperty<string> EndTurnText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> IsAttackAbilityGroupCooldownAlertActive = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<AbstractUnitEntityView> HighlightedUnit = new ReactiveProperty<AbstractUnitEntityView>();

	private bool m_TargetSelectionStarted;

	public readonly ReactiveProperty<ActionBarSlotVM> QuickAccessSlot = new ReactiveProperty<ActionBarSlotVM>();

	public readonly BoolReactiveProperty IsNotControllableCharacter = new BoolReactiveProperty();

	public readonly StringReactiveProperty ControllablePlayerNickname = new StringReactiveProperty();

	private bool m_SlotsUpdateQueued;

	private IFullScreenUIHandler m_FullScreenUIHandlerImplementation;

	private BaseUnitEntity CurrentUnit => CurrentCombatUnit?.Value?.UnitAsBaseUnitEntity;

	public IReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public SurfaceActionBarVM(ReactiveProperty<SurfaceCombatUnitVM> currentUnit)
	{
		AddDisposable(Consumables = new SurfaceActionBarPartConsumablesVM());
		AddDisposable(Weapons = new SurfaceActionBarPartWeaponsVM());
		AddDisposable(Abilities = new SurfaceActionBarPartAbilitiesVM(isInCharScreen: false, IsNotControllableCharacter));
		AddDisposable(SurfaceMomentumVM = new SurfaceMomentumVM());
		AddDisposable(VeilThickness = new VeilThicknessVM());
		CurrentCombatUnit = currentUnit;
		AddDisposable(CurrentCombatUnit.Subscribe(delegate
		{
			OnUnitChanged();
		}));
		m_IsInGame = IsInGameMode(Game.Instance.CurrentMode);
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private void OnUnitChanged()
	{
		UpdateVisibility();
		if (m_IsVisible.Value)
		{
			CurrentUnit.UISettings.TryToInitialize();
			if (!(SurfaceMomentumVM.Unit == CurrentUnit) || !(Consumables.Unit == CurrentUnit) || !(Weapons.Unit == CurrentUnit) || !(Abilities.Unit == CurrentUnit))
			{
				VeilThickness.Update();
				SurfaceMomentumVM.SetUnit(CurrentUnit);
				Consumables.SetUnit(CurrentUnit);
				Weapons.SetUnit(CurrentUnit);
				Abilities.SetUnit(CurrentUnit);
				CheckAnotherPlayerTurn();
			}
		}
	}

	private void UpdateVisibility()
	{
		m_IsVisible.Value = CurrentUnit != null && CurrentUnit.Faction.IsPlayer && m_IsInGame && !m_IsInFullScreenUI && !Game.Instance.TurnController.IsPreparationTurn;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_IsInGame = IsInGameMode(gameMode);
		OnUnitChanged();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private void UpdateSlots(bool onTurnStart = false)
	{
		if (m_SlotsUpdateQueued)
		{
			return;
		}
		m_SlotsUpdateQueued = true;
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			Action<IList<ActionBarSlotVM>> action = delegate(IList<ActionBarSlotVM> slots)
			{
				foreach (ActionBarSlotVM slot in slots)
				{
					slot.UpdateResources();
					if (onTurnStart)
					{
						slot.CloseConvertsOnTurnStart();
					}
				}
			};
			action(Consumables.Slots);
			foreach (SurfaceActionBarPartWeaponSetVM set in Weapons.Sets)
			{
				action(set.AllSlots);
			}
			action(Abilities.Slots);
			action(SurfaceMomentumVM.DesperateMeasureSlots);
			action(SurfaceMomentumVM.HeroicActSlots);
			CheckAnotherPlayerTurn();
			m_SlotsUpdateQueued = false;
		});
	}

	private void CheckAnotherPlayerTurn()
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (!UINetUtility.InLobbyAndPlaying || currentUnit == null)
		{
			IsNotControllableCharacter.Value = false;
			return;
		}
		bool isPlayerFaction = currentUnit.IsPlayerFaction;
		bool flag = ((Game.Instance.CurrentMode == GameModeType.SpaceCombat) ? (!UINetUtility.IsControlMainCharacter()) : (!currentUnit.IsMyNetRole()));
		ControllablePlayerNickname.Value = (PhotonManager.Player.GetNickName(currentUnit.GetPlayer(), out var nickName) ? nickName : string.Empty);
		IsNotControllableCharacter.Value = isPlayerFaction && flag;
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		UpdateSlots();
	}

	public void OnDeliverAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
	{
		UpdateSlots();
	}

	public void HandleLevelUpComplete(bool isChargen)
	{
		OnUnitChanged();
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
		OnUnitChanged();
	}

	public void HandleUISelectionChanged()
	{
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
		if (Game.Instance.CurrentlyLoadedArea.IsPartyArea && (dialog.Type == DialogType.Book || dialog.Type == DialogType.Epilog))
		{
			OnUnitChanged();
		}
	}

	public void HandlePointerEnterActionBarSlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted && ability is MechanicActionBarSlotAbility mechanicActionBarSlotAbility)
		{
			EndTurnText.Value = GetEndTurn(mechanicActionBarSlotAbility.Ability.Blueprint);
		}
	}

	public void HandlePointerExitActionBarSlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted)
		{
			EndTurnText.Value = null;
		}
	}

	public void HandlePointerEnterAttackGroupAbilitySlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted && ability is MechanicActionBarSlotAbility mechanicActionBarSlotAbility)
		{
			IsAttackAbilityGroupCooldownAlertActive.Value = CheckAbilityHasAttackAbilityGroupCooldown(mechanicActionBarSlotAbility.Ability.Blueprint);
		}
	}

	public void HandlePointerExitAttackGroupAbilitySlot(MechanicActionBarSlot ability)
	{
		if (!m_TargetSelectionStarted)
		{
			IsAttackAbilityGroupCooldownAlertActive.Value = false;
		}
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_TargetSelectionStarted = true;
		IsAttackAbilityGroupCooldownAlertActive.Value = CheckAbilityHasAttackAbilityGroupCooldown(ability.Blueprint);
		EndTurnText.Value = GetEndTurn(ability.Blueprint);
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_TargetSelectionStarted = false;
		IsAttackAbilityGroupCooldownAlertActive.Value = false;
		EndTurnText.Value = null;
	}

	private bool CheckAbilityHasAttackAbilityGroupCooldown(BlueprintAbility blueprintAbility)
	{
		return blueprintAbility.AbilityGroups.Any((BlueprintAbilityGroup group) => group.NameSafe() == "WeaponAttackAbilityGroup");
	}

	private string GetEndTurn(BlueprintAbility blueprintAbility)
	{
		WarhammerEndTurn component = blueprintAbility.GetComponent<WarhammerEndTurn>();
		if (component != null)
		{
			return component.clearMPInsteadOfEndingTurn ? UIStrings.Instance.Tooltips.SpendAllMovementPoints : UIStrings.Instance.Tooltips.EndsTurn;
		}
		return string.Empty;
	}

	private bool IsInGameMode(GameModeType gameMode)
	{
		if (!(gameMode == GameModeType.None) && !(gameMode == GameModeType.Default) && !(gameMode == GameModeType.Pause))
		{
			return gameMode == GameModeType.BugReport;
		}
		return true;
	}

	public void OnAreaActivated()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			OnUnitChanged();
		}
	}

	public ActionBarSlotVM GetSuitableSlot(AbstractUnitEntityView unitEntityView)
	{
		if (unitEntityView.Data.IsPlayerFaction)
		{
			return null;
		}
		List<ActionBarSlotVM> allSlots = Weapons.CurrentSet.Value.AllSlots;
		_ = Consumables.Slots;
		return allSlots.FirstOrDefault();
	}

	public void HandleAbilityCooldownStarted(AbilityData ability)
	{
		UpdateSlots();
	}

	public void HandleGroupCooldownRemoved(BlueprintAbilityGroup group)
	{
		UpdateSlots();
	}

	public void HandleCooldownReset()
	{
		UpdateSlots();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		HighlightedUnit.Value = (isHover ? unitEntityView : null);
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		bool isInFullScreenUI = m_IsInFullScreenUI;
		m_IsInFullScreenUI = state && fullScreenUIType != FullScreenUIType.Unknown;
		if (isInFullScreenUI != m_IsInFullScreenUI)
		{
			if (!m_IsInFullScreenUI)
			{
				DelayedInvoker.InvokeInFrames(OnUnitChanged, 1);
			}
			else
			{
				OnUnitChanged();
			}
		}
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		UpdateSlots();
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		UpdateSlots();
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		OnUnitChanged();
	}

	public void HandleEndPreparationTurn()
	{
		OnUnitChanged();
		UpdateSlots();
	}

	public void HandleRoleSet(string entityId)
	{
		CheckAnotherPlayerTurn();
		if (!(CurrentUnit?.UniqueId != entityId))
		{
			UpdateSlots();
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateSlots();
	}

	public void HandleUnitEndInterruptTurn()
	{
		UpdateSlots();
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
		CheckAnotherPlayerTurn();
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		CheckAnotherPlayerTurn();
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

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateSlots();
	}
}
