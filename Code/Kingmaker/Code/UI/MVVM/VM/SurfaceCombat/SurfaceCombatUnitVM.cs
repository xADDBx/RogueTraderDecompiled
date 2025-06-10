using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;

public class SurfaceCombatUnitVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IContinueTurnHandler, IInterruptTurnStartHandler, IRoundStartHandler, IAbilityTargetSelectionUIHandler, ICellAbilityHandler, IInterruptTurnContinueHandler, IUnitPortraitChangedHandler<EntitySubscriber>, IUnitPortraitChangedHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitPortraitChangedHandler, EntitySubscriber>, IChangeAppearanceCloseHandler
{
	public readonly UnitState UnitState;

	public readonly MechanicEntityUIWrapper UnitUIWrapper;

	public readonly ReactiveProperty<bool> IsEnemy = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsNeutral = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsPlayer = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsCurrent = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsUnableToAct = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> WillNotTakeTurn = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> HasControlLossEffects = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsTargetSelection = new ReactiveProperty<bool>(initialValue: false);

	public readonly UnitBuffPartVM UnitBuffs;

	public readonly ReactiveProperty<int> Intiative = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<UnitHealthPartVM> UnitHealthPartVM = new ReactiveProperty<UnitHealthPartVM>();

	public readonly ReactiveProperty<ActionPointsVM> ActionPointVM = new ReactiveProperty<ActionPointsVM>();

	public readonly ReactiveProperty<bool> CanBeShowed = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> SquadCount = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> ShowDifficulty = new ReactiveProperty<bool>();

	public readonly OvertipHitChanceBlockVM OvertipHitChanceBlockVM;

	private readonly PartSquad m_SquadOptional;

	public readonly BoolReactiveProperty IsInSquad = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsSquadLeader = new BoolReactiveProperty();

	public readonly BoolReactiveProperty HasAliveUnitsInSquad = new BoolReactiveProperty();

	public readonly BoolReactiveProperty NeedToShow = new BoolReactiveProperty();

	public readonly IntReactiveProperty SquadGroupIndex = new IntReactiveProperty(-1);

	public readonly ReactiveCommand PortraitChanged = new ReactiveCommand();

	public readonly bool UsedSubtypeIcon;

	public MechanicEntity Unit => UnitUIWrapper.MechanicEntity;

	public BaseUnitEntity UnitAsBaseUnitEntity => Unit as BaseUnitEntity;

	public UnitSquad Squad { get; }

	public bool IsNewUnit { get; private set; }

	public bool HasUnit => Unit != null;

	public string DisplayName => UnitUIWrapper.Name;

	public Sprite SmallPortrait => UnitUIWrapper.SmallPortrait;

	protected SurfaceCombatUnitVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	public SurfaceCombatUnitVM(MechanicEntity entity, bool isCurrent = false)
		: this()
	{
		IsCurrent.Value = isCurrent;
		IsNewUnit = true;
		if (entity == null)
		{
			return;
		}
		MechanicEntity mechanicEntity = entity;
		if (entity is UnitSquad unitSquad)
		{
			mechanicEntity = unitSquad.Leader ?? unitSquad.Units.FirstItem().ToBaseUnitEntity();
			Squad = unitSquad;
		}
		if (mechanicEntity is InitiativePlaceholderEntity initiativePlaceholderEntity)
		{
			mechanicEntity = initiativePlaceholderEntity.Delegate;
		}
		UnitUIWrapper = new MechanicEntityUIWrapper(mechanicEntity);
		UsedSubtypeIcon = UIUtilityUnit.UsedSubtypeIcon(mechanicEntity);
		m_SquadOptional = UnitUIWrapper.MechanicEntity?.GetSquadOptional();
		IsInSquad.Value = UnitUIWrapper.IsInSquad;
		IsSquadLeader.Value = UnitUIWrapper.IsSquadLeader || (m_SquadOptional?.Squad != null && mechanicEntity is BaseUnitEntity baseUnitEntity && m_SquadOptional.Squad.Units.FirstItem() == baseUnitEntity);
		if (Squad == null)
		{
			Squad = m_SquadOptional?.Squad;
		}
		if (UnitUIWrapper.IsInSquad && m_SquadOptional != null)
		{
			SquadGroupIndex.Value = Game.Instance.TurnController.UnitSquads.IndexOf(m_SquadOptional.Squad);
		}
		UnitState = UnitStatesHolderVM.Instance.GetOrCreateUnitState(UnitUIWrapper.MechanicEntity);
		if (UnitState == null)
		{
			return;
		}
		AddDisposable(OvertipHitChanceBlockVM = new OvertipHitChanceBlockVM(UnitState));
		UpdateData();
		AddDisposable(UnitBuffs = new UnitBuffPartVM(UnitAsBaseUnitEntity));
		AddDisposable(UnitHealthPartVM.Value = new UnitHealthPartVM(UnitAsBaseUnitEntity));
		UnitBuffs.SetUnitData(UnitUIWrapper.MechanicEntity);
		AddDisposable(IsCurrent.Subscribe(delegate(bool current)
		{
			if (current)
			{
				ActionPointsVM disposable = (ActionPointVM.Value = new ActionPointsVM(UnitUIWrapper.MechanicEntity));
				AddDisposable(disposable);
				if (UnitAsBaseUnitEntity != null && Unit.IsPlayerFaction && Unit.IsViewActive && Game.Instance.CurrentMode != GameModeType.Cutscene)
				{
					Game.Instance.SelectionCharacter.SetSelected(UnitAsBaseUnitEntity);
				}
			}
			else
			{
				DisposeAndRemove(ActionPointVM);
			}
		}));
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			UpdateHandler();
		}));
		UpdateCanBeShown();
		UILog.VMCreated("InitiativeTrackerUnitVM");
	}

	protected override void DisposeImplementation()
	{
		UILog.VMDisposed("InitiativeTrackerUnitVM");
	}

	private void UpdateHandler()
	{
		if (Game.Instance.CurrentlyLoadedArea != null)
		{
			UpdateCanBeShown();
			ReactiveProperty<bool> showDifficulty = ShowDifficulty;
			MechanicEntityUIWrapper unitUIWrapper = UnitUIWrapper;
			showDifficulty.Value = !unitUIWrapper.IsPlayerFaction && unitUIWrapper.Difficulty > 0 && Game.Instance.TurnController.IsCurrentUnitHunter;
		}
	}

	public void HandleUnitClick(bool isDoubleClick = false)
	{
		ClickUnitHandler.HandleClickControllableUnit(UnitUIWrapper.MechanicEntity, isDoubleClick);
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateIsCurrentUnit();
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		UpdateIsCurrentUnit();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateIsCurrentUnit();
	}

	public void HandleUnitContinueTurn(bool isTurnBased)
	{
		UpdateIsCurrentUnit();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateIsCurrentUnit();
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		UpdateIsCurrentUnit();
	}

	private void UpdateIsCurrentUnit()
	{
		if (!Game.Instance.TurnController.IsPreparationTurn)
		{
			IsCurrent.Value = Game.Instance.TurnController.CurrentUnit == Unit;
		}
	}

	public void HandleCellAbility(List<AbilityTargetUIData> abilityTargets)
	{
		if (UnitState == null)
		{
			return;
		}
		if (abilityTargets.Count == 0)
		{
			UnitState.IsAoETarget.Value = false;
			return;
		}
		UnitState.IsAoETarget.Value = Enumerable.Any(abilityTargets, (AbilityTargetUIData n) => n.Target == Unit);
	}

	public void HandleShow()
	{
		NeedToShow.Value = !NeedToShow.Value;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		if (UnitState != null)
		{
			IsTargetSelection.Value = true;
		}
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		if (UnitState != null)
		{
			IsTargetSelection.Value = false;
		}
	}

	public void UpdateData()
	{
		if (UnitUIWrapper.MechanicEntity == null)
		{
			return;
		}
		IsEnemy.Value = UnitUIWrapper.IsPlayerEnemy;
		IsNeutral.Value = UnitUIWrapper.IsNeutral;
		IsPlayer.Value = UnitUIWrapper.IsPlayerFaction;
		Intiative.Value = (int)UnitUIWrapper.Initiative.Roll;
		if ((bool)m_SquadOptional && m_SquadOptional.Squad != null)
		{
			IsSquadLeader.Value = UnitUIWrapper.IsSquadLeader || m_SquadOptional.Squad.Units.FirstOrDefault((UnitReference x) => !x.Entity.IsDead).Entity == UnitUIWrapper.MechanicEntity;
			HasAliveUnitsInSquad.Value = m_SquadOptional.Squad.Units.Count((UnitReference x) => !x.Entity.IsDead) > 1;
			SquadCount.Value = m_SquadOptional.Squad.Units.Count((UnitReference x) => !x.Entity.IsDead);
		}
		UnitBuffs?.UpdateData();
		UpdateCanActStates();
		UpdateIsCurrentUnit();
		IsNewUnit = false;
	}

	public void SetMouseHighlighted(bool value)
	{
		if (NeedToShow.Value)
		{
			UnitAsBaseUnitEntity.View.MouseHighlighted = value;
			return;
		}
		if (Squad == null)
		{
			UnitAsBaseUnitEntity.View.MouseHighlighted = value;
			return;
		}
		foreach (UnitReference unit in Squad.Units)
		{
			unit.ToBaseUnitEntity().View.MouseHighlighted = value;
		}
	}

	private void UpdateCanActStates()
	{
		if (Unit is BaseUnitEntity baseUnitEntity)
		{
			IsUnableToAct.Value = !baseUnitEntity.State.CanActMechanically;
			WillNotTakeTurn.Value = !WillTakeTurn(baseUnitEntity);
			HasControlLossEffects.Value = baseUnitEntity.HasControlLossEffects();
		}
	}

	private static bool WillTakeTurn(BaseUnitEntity unit)
	{
		if (unit.State.HasCondition(UnitCondition.Stunned))
		{
			return false;
		}
		if (unit.State.IsHelpless)
		{
			return false;
		}
		if (unit.State.IsProne)
		{
			return false;
		}
		return true;
	}

	public void InvokeUnitViewHighlight(bool state)
	{
		if (UnitUIWrapper.MechanicEntity is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.View.HandleHoverChange(state);
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateIsCurrentUnit();
	}

	private void UpdateCanBeShown()
	{
		if (RootUIContext.Instance.FullScreenUIType != 0)
		{
			CanBeShowed.Value = false;
		}
		else
		{
			CanBeShowed.Value = Game.Instance.CurrentMode == GameModeType.Default || Game.Instance.CurrentMode == GameModeType.None || Game.Instance.CurrentMode == GameModeType.Pause || Game.Instance.CurrentMode == GameModeType.BugReport || Game.Instance.CurrentMode == GameModeType.GlobalMap || Game.Instance.CurrentMode == GameModeType.StarSystem || Game.Instance.CurrentMode == GameModeType.SpaceCombat;
		}
	}

	public void HandlePortraitChanged()
	{
		PortraitChanged.Execute();
	}

	public void HandleCloseChangeAppearance()
	{
		PortraitChanged.Execute();
	}
}
