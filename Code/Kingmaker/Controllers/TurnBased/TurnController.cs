using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype.PsychicPowers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.Signals;
using Kingmaker.SpaceCombat.MeteorStream;
using Kingmaker.UI.Common;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UI.Sound;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.TurnBased;

public class TurnController : IControllerEnable, IController, IControllerDisable, IControllerStart, IControllerStop, IControllerTick, IAreaHandler, ISubscriber, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>
{
	private const float OutOfCombatSpeedMod = 5f;

	private const int FirstCombatRound = 1;

	public const int DeploymentEnemyRestrictionRadius = 1;

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("TurnController");

	private static readonly TimeSpan OutOfCombatRoundDuration = 5f.Seconds();

	private readonly HashSet<MechanicEntity> m_WaitingForInitiativeEntities = new HashSet<MechanicEntity>();

	private readonly List<MechanicEntity> m_JoinedThisTickEntities = new List<MechanicEntity>();

	public readonly MomentumController MomentumController = new MomentumController();

	public readonly VeilThicknessCounter VeilThicknessCounter = new VeilThicknessCounter();

	private bool m_IsControllerStarted;

	private bool m_IsControllerActive;

	private TimeSpan? m_UnableToActUnitForceTurnEndTime;

	private SignalWrapper m_StartBattleSignal;

	private readonly WeakReference<TurnDataPart> m_Data = new WeakReference<TurnDataPart>(null);

	private bool m_OnAreaBeginUnloading;

	private bool m_IsFirstTick;

	public bool TbActive
	{
		get
		{
			if (m_IsControllerStarted)
			{
				return Data.InCombat;
			}
			return false;
		}
	}

	public bool TurnBasedModeActive
	{
		get
		{
			if (m_IsControllerStarted)
			{
				return Data.InCombat;
			}
			return false;
		}
	}

	public bool InCombat => Data.InCombat;

	public bool IsPlayerTurn
	{
		get
		{
			if (m_IsControllerStarted)
			{
				MechanicEntity currentUnit = CurrentUnit;
				if (currentUnit != null && currentUnit.IsInPlayerParty)
				{
					return !IsRoamingTurn;
				}
			}
			return false;
		}
	}

	public bool IsAiTurn
	{
		get
		{
			if (m_IsControllerStarted)
			{
				MechanicEntity currentUnit = CurrentUnit;
				if (currentUnit != null && !currentUnit.IsInPlayerParty)
				{
					return !IsRoamingTurn;
				}
			}
			return false;
		}
	}

	public bool IsUltimateAbilityUsedThisRound
	{
		get
		{
			return Data.IsUltimateAbilityUsedThisRound;
		}
		private set
		{
			Data.IsUltimateAbilityUsedThisRound = value;
		}
	}

	public TurnOrderQueue TurnOrder => Data.TurnOrder;

	public IEnumerable<MechanicEntity> UnitsAndSquadsByInitiativeForCurrentTurn
	{
		get
		{
			if (!m_IsControllerStarted || TurnOrder?.CurrentRoundUnitsOrder == null)
			{
				return Array.Empty<MechanicEntity>();
			}
			return TurnOrder?.CurrentRoundUnitsOrder;
		}
	}

	public IEnumerable<MechanicEntity> UnitsAndSquadsByInitiativeForNextTurn
	{
		get
		{
			if (!m_IsControllerStarted || TurnOrder?.NextRoundUnitsOrder == null)
			{
				return Array.Empty<MechanicEntity>();
			}
			return TurnOrder?.NextRoundUnitsOrder;
		}
	}

	public bool IsRoamingTurn => TurnOrder.CurrentTurnType == CombatTurnType.Roaming;

	public bool IsManualCombatTurn => TurnOrder.CurrentTurnType == CombatTurnType.ManualCombat;

	public bool IsPreparationTurn => TurnOrder.CurrentTurnType == CombatTurnType.Preparation;

	public bool NeedDeploymentPhase => Game.Instance.CurrentMode != GameModeType.SpaceCombat;

	public bool IsDeploymentAllowed => UnitsInCombat.Any((MechanicEntity u) => u.IsDirectlyControllable && !(u.GetCombatStateOptional()?.Surprised ?? false));

	public IEnumerable<MechanicEntity> AllUnits
	{
		get
		{
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
			{
				if (!allBaseAwakeUnit.State.ControlledByDirector && !allBaseAwakeUnit.IsExtra && !allBaseAwakeUnit.Features.RemoveFromInitiative)
				{
					yield return allBaseAwakeUnit;
				}
			}
			foreach (UnitSquad item in UnitSquad.All)
			{
				yield return item;
			}
			foreach (MeteorStreamEntity meteorStreamEntity in Game.Instance.State.MeteorStreamEntities)
			{
				yield return meteorStreamEntity;
			}
			foreach (InitiativePlaceholderEntity item2 in InitiativePlaceholderEntity.All)
			{
				yield return item2;
			}
		}
	}

	public IEnumerable<MechanicEntity> UnitsInCombat => AllUnits.Where((MechanicEntity i) => i.IsInCombat);

	[CanBeNull]
	public MechanicEntity CurrentUnit => TurnOrder.CurrentUnit;

	public bool IsCurrentUnitHunter { get; private set; }

	public int CombatRound
	{
		get
		{
			if (!m_IsControllerStarted)
			{
				return 0;
			}
			return Data.CombatRound;
		}
	}

	public int GameRound => Data.GameRound;

	public bool EndTurnRequested => Data.EndTurnRequested;

	private TurnDataPart Data
	{
		get
		{
			if (m_Data.TryGetTarget(out var target))
			{
				return target;
			}
			TurnDataPart orCreate = Game.Instance.Player.GetOrCreate<TurnDataPart>();
			m_Data.SetTarget(orCreate);
			return orCreate;
		}
	}

	private bool CanEndTurn => CurrentUnit.GetCombatStateOptional()?.CanEndTurn() ?? true;

	public IEnumerable<UnitSquad> UnitSquads => UnitsAndSquadsByInitiativeForCurrentTurn.OfType<UnitSquad>();

	public bool IsSpaceCombat => Game.Instance.CurrentMode == GameModeType.SpaceCombat;

	public void OnStart()
	{
		m_IsControllerStarted = true;
		m_IsFirstTick = true;
		m_WaitingForInitiativeEntities.Clear();
		if (TurnBasedModeActive)
		{
			TurnOrder.RestoreCurrentUnit();
		}
		if (Data.InCombat && Game.Instance.TurnController.IsPreparationTurn && NeedDeploymentPhase)
		{
			BeginPreparationTurn(IsDeploymentAllowed);
		}
		EventBus.RaiseEvent(delegate(ITurnBasedModeStartHandler h)
		{
			h.HandleTurnBasedModeStarted();
		});
	}

	public void OnEnable()
	{
		MomentumController.TurnController = this;
		EventBus.Subscribe(VeilThicknessCounter);
		VeilThicknessCounter.OnEnable();
		MomentumController.OnEnable();
		EventBus.Subscribe(MomentumController);
		if (Data.InCombat)
		{
			EventBus.RaiseEvent(delegate(ITurnBasedModeResumeHandler h)
			{
				h.HandleTurnBasedModeResumed();
			});
		}
	}

	public void OnDisable()
	{
		UnitCombatLeaveController.TickGroups(ignoreTimer: true);
		if (Data.InCombat && (CurrentUnit == null || Data.EndTurnRequested) && !m_OnAreaBeginUnloading)
		{
			if (IsRoamingTurn)
			{
				EndRoamingUnitsTurn();
			}
			EndUnitTurn(CurrentUnit, isTurnBased: true, Data.EndTurnRequested);
		}
		EventBus.Unsubscribe(VeilThicknessCounter);
		EventBus.Unsubscribe(MomentumController);
		MomentumController.OnDisable();
		Game.Instance.Player.UISettings.StopSpeedUp();
	}

	public void OnStop()
	{
		ExitTb();
		Data.LastTurnTime = TimeSpan.Zero;
		m_IsControllerStarted = false;
		m_OnAreaBeginUnloading = false;
	}

	public bool CanFinishDeploymentPhase()
	{
		if (!IsDeploymentAllowed)
		{
			return true;
		}
		BaseUnitEntity[] array = Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity i) => i.IsInCombat && i.IsPlayerEnemy).ToArray();
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.CombatState.StartedCombatNearEnemy)
			{
				continue;
			}
			BaseUnitEntity[] array2 = array;
			foreach (BaseUnitEntity other in array2)
			{
				if (!item.IsDeadOrUnconscious && item.DistanceToInCells(other) <= 1)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void EnterTb()
	{
		if (Data.InCombat)
		{
			return;
		}
		Data.InCombat = true;
		EventBus.RaiseEvent(delegate(ITurnBasedModeHandler h)
		{
			h.HandleTurnBasedModeSwitched(isTurnBased: true);
		});
		MomentumController.InitializeGroups();
		InitiativeHelper.Update();
		AddUnitsToCombat();
		NetService.Instance.CancelCurrentCommands();
		if (!Game.Instance.AbilityExecutor.Abilities.Empty())
		{
			Game.Instance.AbilityExecutor.Abilities.ForEach(delegate(AbilityExecutionProcess ability)
			{
				ability.InstantDeliver();
			});
		}
		NextRound(isFirst: true);
		if (NeedDeploymentPhase)
		{
			BeginPreparationTurn(IsDeploymentAllowed);
		}
		else
		{
			NextTurnTB();
		}
		HandleJoinedThisTickEntities();
	}

	private void ExitTb()
	{
		if (!Data.InCombat)
		{
			return;
		}
		Data.InCombat = false;
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			allBaseAwakeUnit.Commands.InterruptAllInterruptible();
		}
		EventBus.RaiseEvent(delegate(ITurnBasedModeHandler h)
		{
			h.HandleTurnBasedModeSwitched(isTurnBased: false);
		});
		foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
		{
			for (int i = 0; i < unitGroup.Count; i++)
			{
				BaseUnitEntity baseUnitEntity = unitGroup[i];
				if (baseUnitEntity != null && !baseUnitEntity.IsInCombat)
				{
					baseUnitEntity.UnequipItemsWithFailedRestrictions();
				}
			}
		}
		Data.CombatRound = 0;
		TurnOrder.Clear();
		Data.EndTurnRequested = false;
		MomentumController.ClearGroups();
		m_UnableToActUnitForceTurnEndTime = null;
		Game.Instance.Player.UISettings.StopSpeedUp();
		InitiativeHelper.Update();
	}

	private void HandleJoinedThisTickEntities()
	{
		foreach (MechanicEntity joinedThisTickEntity in m_JoinedThisTickEntities)
		{
			PrepareUnitForNewTurn(joinedThisTickEntity, isTurnBased: true, setPreparedRound: false);
			EventBus.RaiseEvent((IMechanicEntity)joinedThisTickEntity, (Action<IEntityJoinTBCombat>)delegate(IEntityJoinTBCombat h)
			{
				h.HandleEntityJoinTBCombat();
			}, isCheckRuntime: true);
		}
		m_JoinedThisTickEntities.Clear();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!TbActive && IsInTurnBasedCombat())
		{
			EnterTb();
		}
		if (m_IsFirstTick)
		{
			ApplyPostLoadFixes();
			m_IsFirstTick = false;
		}
		SetTime();
		if (!Data.InCombat)
		{
			TickRoundRT();
			return;
		}
		TryRollInitiative();
		TrySelectCurrentUnitInUI();
		HandleCurrentUnitUnableToAct();
		MechanicEntity currentUnit = CurrentUnit;
		if (currentUnit == null || !currentUnit.IsInGame || Data.EndTurnRequested)
		{
			EndUnitTurn(CurrentUnit, isTurnBased: true, Data.EndTurnRequested);
		}
		HandleJoinedThisTickEntities();
		if (!m_StartBattleSignal.IsEmpty && SignalService.Instance.CheckReady(ref m_StartBattleSignal))
		{
			ForceEndPreparationTurn();
		}
	}

	private void TickRoundRT()
	{
		if (Game.Instance.Player.GameTime - Data.LastTurnTime > OutOfCombatRoundDuration)
		{
			Data.LastTurnTime = Game.Instance.Player.GameTime;
			NextRoundRT();
		}
	}

	private void TryRollInitiative()
	{
		m_WaitingForInitiativeEntities.AddRange(UnitsInCombat.Where((MechanicEntity unit) => unit.Initiative.Empty));
		if (m_WaitingForInitiativeEntities.Empty())
		{
			return;
		}
		try
		{
			bool relax = !IsSpaceCombat && UnitsInCombat.All((MechanicEntity i) => i.Initiative.Empty);
			InitiativeHelper.Roll(m_WaitingForInitiativeEntities, relax);
		}
		finally
		{
			m_JoinedThisTickEntities.AddRange(m_WaitingForInitiativeEntities);
			m_WaitingForInitiativeEntities.Clear();
		}
	}

	private void ScheduleInitiativeRoll(BaseUnitEntity unit)
	{
		PartSquad squadOptional = unit.GetSquadOptional();
		if (squadOptional != null)
		{
			UnitSquad squad = squadOptional.Squad;
			if (squad != null && squad.Initiative.Empty)
			{
				m_WaitingForInitiativeEntities.Add(squad);
			}
		}
		if (unit.Initiative.Empty)
		{
			m_WaitingForInitiativeEntities.Add(unit);
		}
	}

	private void TrySelectCurrentUnitInUI()
	{
		if (!IsPreparationTurn && CurrentUnit.IsDirectlyControllable() && CurrentUnit is BaseUnitEntity unit && (Game.Instance.SelectionCharacter.SelectedUnits.Count != 1 || Game.Instance.SelectionCharacter.FirstSelectedUnit != CurrentUnit))
		{
			Game.Instance.SelectionCharacter.SetSelected(unit, force: true);
		}
	}

	public void TryEndPlayerTurnManually()
	{
		if (!IsPlayerTurn)
		{
			return;
		}
		if (CanEndTurn)
		{
			if (CurrentUnit.IsMyNetRole())
			{
				UISounds.Instance.Sounds.Combat.EndTurn.Play();
				Game.Instance.GameCommandQueue.EndTurnManually(CurrentUnit);
			}
		}
		else
		{
			ShowShouldFlyFurtherMessage();
		}
	}

	public void TryEndPlayerTurn(EntityRef<MechanicEntity> mechanicEntity)
	{
		if (IsPlayerTurn && mechanicEntity != CurrentUnit)
		{
			Logger.Warning("Attempt to end the turn of another unit! Current=" + CurrentUnit?.UniqueId + ", expected=" + mechanicEntity.Id);
		}
		else
		{
			TryEndPlayerTurn();
		}
	}

	private void TryEndPlayerTurn()
	{
		if (IsPlayerTurn && CanEndTurn)
		{
			Game.Instance.ClickEventsController.ClearPointerMode();
			RequestEndTurn();
		}
		else
		{
			ShowShouldFlyFurtherMessage();
		}
	}

	private void ShowShouldFlyFurtherMessage()
	{
		LocalizedString restrictionText = LocalizedTexts.Instance.Reasons.ShouldFlyFurtherToEndTurn;
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(restrictionText, addToLog: false);
		});
		CombatHUDRenderer.Instance.HighlightSpaceMovementAreaThirdPhase();
	}

	public void RequestEndTurn()
	{
		Data.EndTurnRequested = TurnBasedModeActive;
		m_UnableToActUnitForceTurnEndTime = null;
	}

	public void EndRoamingUnitsTurn()
	{
		TurnOrder.EndRoamingUnitsTurn();
	}

	private void SetTime()
	{
		float playerTimeScale = 1f;
		if (IsRoamingTurn)
		{
			playerTimeScale = 5f;
		}
		else if (IsAiTurn)
		{
			MechanicEntity currentUnit = CurrentUnit;
			playerTimeScale = ((currentUnit != null && currentUnit.IsInFogOfWar) ? 16f : Game.Instance.AiBrainController.AiAbilitySpeedMod);
		}
		Game.Instance.TimeController.PlayerTimeScale = playerTimeScale;
	}

	private void NextRound(bool isFirst)
	{
		EventBus.RaiseEvent(delegate(IRoundEndHandler h)
		{
			h.HandleRoundEnd(TurnBasedModeActive);
		});
		if (TurnBasedModeActive)
		{
			Data.CombatRound = (isFirst ? 1 : (Data.CombatRound + 1));
			IsUltimateAbilityUsedThisRound = false;
		}
		Data.GameRound++;
		EventBus.RaiseEvent(delegate(IRoundStartHandler h)
		{
			h.HandleRoundStart(TurnBasedModeActive);
		});
	}

	private void NextRoundRT()
	{
		foreach (MechanicEntity allUnit in AllUnits)
		{
			if (!(allUnit is UnitEntity { IsExtra: not false }))
			{
				EndUnitTurn(allUnit, isTurnBased: false);
			}
		}
		NextRound(isFirst: false);
		foreach (MechanicEntity allUnit2 in AllUnits)
		{
			if (!(allUnit2 is UnitEntity { IsExtra: not false }))
			{
				StartUnitTurn(allUnit2, isTurnBased: false);
			}
		}
	}

	private void StartUnitTurn([NotNull] MechanicEntity entity, bool isTurnBased, InterruptionData interruptionData = null)
	{
		if (entity is UnitSquad unitSquad)
		{
			unitSquad.Units.ForEach(delegate(UnitReference u)
			{
				StartUnitTurnInternal(u.ToBaseUnitEntity(), isTurnBased);
			});
		}
		else if (!entity.IsInSquad)
		{
			StartUnitTurnInternal(entity, isTurnBased);
		}
		if (entity.Initiative.InterruptingOrder > 0)
		{
			EventBus.RaiseEvent((IMechanicEntity)entity, (Action<IInterruptTurnStartHandler>)delegate(IInterruptTurnStartHandler h)
			{
				h.HandleUnitStartInterruptTurn(interruptionData);
			}, isCheckRuntime: true);
		}
		else
		{
			EventBus.RaiseEvent((IMechanicEntity)entity, (Action<ITurnStartHandler>)delegate(ITurnStartHandler h)
			{
				h.HandleUnitStartTurn(isTurnBased);
			}, isCheckRuntime: true);
		}
		IsCurrentUnitHunter = (entity?.GetProgressionOptional()?.AllCareerPaths?.Any(((BlueprintCareerPath Blueprint, int Rank) c) => c.Blueprint.IsHunter)).GetValueOrDefault();
	}

	private void StartUnitTurnInternal(MechanicEntity entity, bool isTurnBased)
	{
		if (isTurnBased && entity is UnitEntity unit)
		{
			unit.SnapToGrid();
		}
		if (entity.IsDirectlyControllable)
		{
			Game.Instance.Player.UISettings.StopSpeedUp();
		}
		if (entity.Initiative.InterruptingOrder < 1 && entity.Initiative.WasPreparedForRound < CombatRound)
		{
			PrepareUnitForNewTurn(entity, isTurnBased);
			entity.GetAbilityCooldownsOptional()?.RemoveHandAbilityGroupsCooldown();
		}
		else if (entity.Initiative.InterruptingOrder > 0)
		{
			entity.GetAbilityCooldownsOptional()?.SaveCooldownData();
			entity.GetAbilityCooldownsOptional()?.ResetCooldowns();
		}
	}

	private void EndUnitTurn([CanBeNull] MechanicEntity unit, bool isTurnBased, bool isEndOfTurnRequested = false)
	{
		m_UnableToActUnitForceTurnEndTime = null;
		if (isTurnBased)
		{
			if (unit is UnitEntity unit2)
			{
				unit2.SnapToGrid();
			}
			if (unit is UnitSquad { IsInCombat: not false } unitSquad)
			{
				unitSquad.Units.ForEach(delegate(UnitReference i)
				{
					BaseUnitEntity baseUnitEntity = i.Entity.ToBaseUnitEntity();
					if (baseUnitEntity != null && !baseUnitEntity.AnimationManager.InCover)
					{
						baseUnitEntity.AnimationManager.CreateMainHandAttackHandlerForPrepare();
					}
				});
			}
			if (unit != null && unit.IsInSquad)
			{
				unit.MaybeAnimationManager?.CurrentMainHandAttackForPrepare?.Release();
			}
			foreach (MechanicEntity item in UnitsInCombat)
			{
				if (!HandleEntityCommands(item, isEndOfTurnRequested))
				{
					return;
				}
			}
			PartReplaceUnitTransition partReplaceUnitTransition = unit?.GetReplaceUnitTransitionOptional();
			if (partReplaceUnitTransition != null && partReplaceUnitTransition.IsFromOwner && partReplaceUnitTransition.ToUnit != null && !partReplaceUnitTransition.IsFinished)
			{
				return;
			}
		}
		Data.EndTurnRequested = false;
		if (unit != null)
		{
			if (unit.Initiative.WasPreparedForRound != CombatRound && unit.Initiative.InterruptingOrder == 0)
			{
				unit.Initiative.WasPreparedForRound = CombatRound;
			}
			if (unit.Initiative.InterruptingOrder > 0)
			{
				unit.GetAbilityCooldownsOptional()?.RestoreCooldownData();
				unit.Initiative.InterruptingOrder = 0;
				EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IInterruptTurnEndHandler>)delegate(IInterruptTurnEndHandler h)
				{
					h.HandleUnitEndInterruptTurn();
				}, isCheckRuntime: true);
			}
			else
			{
				TickAbilityCooldowns(unit, interrupt: false);
				if (!unit.Features.DoesNotCountTurns && TurnOrder?.CurrentRoundUnitsOrder.FirstOrDefault() == unit)
				{
					unit.Initiative.LastTurn = GameRound;
				}
				EventBus.RaiseEvent((IMechanicEntity)unit, (Action<ITurnEndHandler>)delegate(ITurnEndHandler h)
				{
					h.HandleUnitEndTurn(TurnBasedModeActive);
				}, isCheckRuntime: true);
			}
		}
		if (isTurnBased)
		{
			NextTurnTB();
		}
		static bool HandleEntityCommands(MechanicEntity e, bool endOfTurnRequested)
		{
			PartUnitCommands partUnitCommands = e?.GetCommandsOptional();
			if (partUnitCommands != null)
			{
				if (!partUnitCommands.Empty && !e.IsDead && !endOfTurnRequested)
				{
					return false;
				}
				partUnitCommands.InterruptAllInterruptible();
			}
			return true;
		}
	}

	private void NextTurnTB()
	{
		bool nextRound;
		bool endOfCombat;
		CombatTurnType turnType;
		MechanicEntity entity = TurnOrder.NextTurn(out nextRound, out endOfCombat, out turnType);
		if (turnType == CombatTurnType.Roaming)
		{
			return;
		}
		if (endOfCombat)
		{
			ExitTb();
			return;
		}
		if (nextRound)
		{
			NextRound(isFirst: false);
		}
		if (CurrentUnit != null)
		{
			StartUnitTurn(entity, isTurnBased: true);
		}
	}

	public static void TickAbilityCooldowns([CanBeNull] MechanicEntity unit, bool interrupt)
	{
		unit?.GetAbilityCooldownsOptional()?.TickCooldowns(interrupt);
	}

	private void PrepareUnitForNewTurn(MechanicEntity entity, bool isTurnBased, bool setPreparedRound = true)
	{
		if (setPreparedRound)
		{
			entity.Initiative.WasPreparedForRound = CombatRound;
		}
		entity.GetCombatStateOptional()?.PrepareForNewTurn(isTurnBased);
		if (isTurnBased)
		{
			entity.GetCommandsOptional()?.InterruptAllInterruptible();
		}
	}

	private void HandleUnitStartCombat(BaseUnitEntity unit)
	{
		if (TbActive)
		{
			if (unit.IsDead)
			{
				Logger.Error("Trying to force dead unit {0} to combat", unit);
				return;
			}
			PrepareUnitForNewTurn(unit, TbActive, setPreparedRound: false);
			MomentumController.HandleUnitJoinCombat(unit);
			TickAbilityCooldowns(unit, interrupt: false);
			ScheduleInitiativeRoll(unit);
		}
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		HandleUnitJoinCombat(EventInvokerExtensions.BaseUnitEntity);
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		HandleUnitLeaveCombat(EventInvokerExtensions.BaseUnitEntity);
	}

	private void AddUnitsToCombat()
	{
		List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
		foreach (BaseUnitEntity item in UnitsInCombat.OfType<BaseUnitEntity>())
		{
			if (item.Initiative.Empty)
			{
				HandleUnitStartCombat(item);
				list.Add(item);
			}
		}
		list.SnapToGrid();
		TryRollInitiative();
	}

	private void HandleUnitJoinCombat(BaseUnitEntity unit)
	{
		HandleUnitStartCombat(unit);
		unit.SnapToGrid();
	}

	private void HandleUnitLeaveCombat(BaseUnitEntity unit)
	{
		unit.Initiative.Clear();
		RemoveUnit(unit);
	}

	private void HandleCurrentUnitUnableToAct()
	{
		UnitSquad unitSquad = CurrentUnit as UnitSquad;
		if (unitSquad != null && unitSquad.Units.HasItem((UnitReference i) => i.Entity?.CanAct ?? false))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = CurrentUnit as BaseUnitEntity;
		if ((baseUnitEntity != null && baseUnitEntity.State.CanAct) || (baseUnitEntity == null && unitSquad == null))
		{
			return;
		}
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		if (!m_UnableToActUnitForceTurnEndTime.HasValue)
		{
			m_UnableToActUnitForceTurnEndTime = gameTime + 1.Seconds();
		}
		else
		{
			if (!(m_UnableToActUnitForceTurnEndTime <= gameTime))
			{
				return;
			}
			if (baseUnitEntity != null)
			{
				EventBus.RaiseEvent((IEntity)baseUnitEntity, (Action<IUnitMissedTurnHandler>)delegate(IUnitMissedTurnHandler h)
				{
					h.HandleOnMissedTurn();
				}, isCheckRuntime: true);
			}
			RequestEndTurn();
			m_UnableToActUnitForceTurnEndTime = null;
		}
	}

	private void RemoveUnit(BaseUnitEntity unit)
	{
		MomentumController.HandleUnitLeaveCombat(unit);
		UnitPathManager.Instance.RemovePath(unit);
		if (CurrentUnit == unit)
		{
			RequestEndTurn();
		}
		PartSquad squadOptional = unit.GetSquadOptional();
		if (squadOptional == null)
		{
			return;
		}
		UnitSquad squad = squadOptional.Squad;
		if (squad != null && squad.Units.AllItems((UnitReference i) => i.Entity?.ToBaseUnitEntity().Initiative.Empty ?? true))
		{
			squad.Initiative.Clear();
			if (CurrentUnit == squad)
			{
				RequestEndTurn();
			}
		}
	}

	public void InterruptCurrentTurn(MechanicEntity unit, MechanicEntity source, InterruptionData interruptionData)
	{
		if (!IsInTurnBasedCombat())
		{
			return;
		}
		if (interruptionData.WaitForCommandsToFinish)
		{
			PartUnitCommands commandsOptional = CurrentUnit.GetCommandsOptional();
			if (commandsOptional != null)
			{
				commandsOptional.AddToQueue(new UnitInterruptTurnParams(unit, source, interruptionData));
				return;
			}
		}
		InterruptCurrentTurnByCommand(unit, source, interruptionData);
	}

	public void InterruptCurrentTurnByCommand(MechanicEntity unit, MechanicEntity source, InterruptionData interruptionData)
	{
		if (!IsInTurnBasedCombat())
		{
			return;
		}
		using (ContextData<InterruptTurnData>.Request().Setup(unit, source))
		{
			EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IInterruptCurrentTurnHandler>)delegate(IInterruptCurrentTurnHandler h)
			{
				h.HandleOnInterruptCurrentTurn();
			}, isCheckRuntime: true);
		}
		TurnOrder.InterruptCurrentUnit(unit);
		StartUnitTurn(unit, isTurnBased: true, interruptionData);
		Data.EndTurnRequested = false;
	}

	public static bool IsInTurnBasedCombat()
	{
		if (Game.Instance.Player != null && Game.Instance.Player.IsInCombat)
		{
			return Game.Instance.CurrentMode != GameModeType.Cutscene;
		}
		return false;
	}

	[Cheat(Name = "end_turn")]
	public static void TryEndPlayerTurnStatic()
	{
		Game.Instance.TurnController.TryEndPlayerTurn();
	}

	public void OnAreaBeginUnloading()
	{
		m_OnAreaBeginUnloading = true;
	}

	public void OnAreaDidLoad()
	{
		m_OnAreaBeginUnloading = false;
	}

	public void NextRoundForManualCombat()
	{
		if (!IsManualCombatTurn || !TurnBasedModeActive)
		{
			throw new InvalidOperationException("TurnController.NextRoundForManualCombat() is invalid outside of manual combat");
		}
		NextRoundRT();
	}

	public void BeginPreparationTurn(bool canDeploy)
	{
		if (!TurnBasedModeActive || CombatRound > 1)
		{
			throw new InvalidOperationException("BeginPreparationTurn");
		}
		m_StartBattleSignal = SignalService.Instance.RegisterNext();
		TurnOrder.BeginPreparationTurn();
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			item.GetCombatStateOptional()?.SetBluePoint(BlueprintWarhammerRoot.Instance.CombatRoot.DistanceInPreparationTurn);
		}
		BaseUnitEntity[] array = Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity i) => i.IsInCombat && i.IsPlayerEnemy).ToArray();
		foreach (BaseUnitEntity item2 in Game.Instance.Player.Party)
		{
			BaseUnitEntity[] array2 = array;
			foreach (BaseUnitEntity other in array2)
			{
				if (item2.CombatState.StartedCombatNearEnemy = item2.DistanceToInCells(other) <= 1)
				{
					break;
				}
			}
		}
		AddPreparationTurnVisualEffect();
		EventBus.RaiseEvent(delegate(IPreparationTurnBeginHandler h)
		{
			h.HandleBeginPreparationTurn(canDeploy);
		});
	}

	public void RequestEndPreparationTurn()
	{
		SignalService.Instance.CheckReadyOrSend(ref m_StartBattleSignal);
	}

	public void AddPreparationTurnVisualEffect()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			item.AddFact((BlueprintBuff)BlueprintRoot.Instance.FxRoot.PreparationTurnVisualBuff);
		}
	}

	public void RemovePreparationTurnVisualEffect()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			item.Facts.Remove((BlueprintBuff)BlueprintRoot.Instance.FxRoot.PreparationTurnVisualBuff);
		}
	}

	public void ForceEndPreparationTurn()
	{
		if (!TurnBasedModeActive || !IsPreparationTurn)
		{
			PFLog.Default.Error(string.Format("{0} invalid operation: TurnBasedModeActive={1} IsPreparationTurn={2}", "ForceEndPreparationTurn", TurnBasedModeActive, IsPreparationTurn));
			return;
		}
		RemovePreparationTurnVisualEffect();
		TurnOrder.EndPreparationTurn();
		EventBus.RaiseEvent(delegate(IPreparationTurnEndHandler h)
		{
			h.HandleEndPreparationTurn();
		});
	}

	public void BeginManualCombat()
	{
		if (TurnBasedModeActive)
		{
			throw new InvalidOperationException("BeginManualCombat");
		}
		UnitCombatJoinController.JoinPartyToCombat();
		EnterTb();
		TurnOrder.BeginManualCombat();
	}

	public void EndManualCombat()
	{
		TurnOrder.EndManualCombat();
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		TryRestrictAbilityTillNextRound(context);
	}

	private static void TryRestrictAbilityTillNextRound(AbilityExecutionContext context)
	{
		BlueprintAbility blueprint = context.Ability.Blueprint;
		if ((blueprint != null && (blueprint.IsHeroicAct || blueprint.IsDesperateMeasure)) ? true : false)
		{
			Game.Instance.TurnController.IsUltimateAbilityUsedThisRound = true;
		}
	}

	public bool GetStartBattleProgress(out int current, out int target, out NetPlayerGroup playerGroup)
	{
		return SignalService.Instance.GetProgress(m_StartBattleSignal, out current, out target, out playerGroup);
	}

	private void ApplyPostLoadFixes()
	{
		try
		{
			if (!IsSpaceCombat)
			{
				foreach (BaseUnitEntity unit in Game.Instance.State.AllBaseUnits.All)
				{
					if (unit.IsInCombat && unit.Suppressed && Game.Instance.Player.Group.Memory.Contains(unit))
					{
						BaseUnitEntity baseUnitEntity = Game.Instance.Player.Party.OrderBy((BaseUnitEntity i) => i.DistanceToInCells(unit)).FirstOrDefault();
						unit.Position = baseUnitEntity?.Position ?? Game.Instance.Player.MainCharacter.Entity.Position;
						unit.SnapToGrid();
					}
				}
			}
			if (!IsInTurnBasedCombat() || IsSpaceCombat)
			{
				return;
			}
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
			{
				if (allBaseAwakeUnit.IsInCombat && !allBaseAwakeUnit.IsDead)
				{
					allBaseAwakeUnit.SnapToGrid();
				}
			}
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, null);
		}
	}
}
