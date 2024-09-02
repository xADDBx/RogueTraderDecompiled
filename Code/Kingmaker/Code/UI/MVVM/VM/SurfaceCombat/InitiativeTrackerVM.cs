using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;

public class InitiativeTrackerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IRoundStartHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IInGameHandler, ISubscriber<IEntity>, IUnitDirectHoverUIHandler, IUnitMountHandler, IUnitBecameVisibleHandler, IUnitBecameInvisibleHandler, IInitiativeChangeHandler, IInitiativeTrackerShowGroup
{
	public List<InitiativeTrackerUnitVM> Units = new List<InitiativeTrackerUnitVM>();

	public Dictionary<int, InitiativeTrackerSquadLeaderVM> Dictionary = new Dictionary<int, InitiativeTrackerSquadLeaderVM>();

	public int RoundIndex;

	public ReactiveProperty<InitiativeTrackerUnitVM> HoveredUnit = new ReactiveProperty<InitiativeTrackerUnitVM>();

	public ReactiveProperty<InitiativeTrackerUnitVM> SquadLeaderUnit = new ReactiveProperty<InitiativeTrackerUnitVM>();

	public InitiativeTrackerUnitVM RoundVM;

	public readonly ReactiveCommand UnitsUpdated = new ReactiveCommand();

	public ReactiveProperty<bool> CanInterruptMovement = new ReactiveProperty<bool>();

	public IntReactiveProperty RoundCounter = new IntReactiveProperty();

	public ReactiveProperty<InitiativeTrackerUnitVM> CurrentUnit = new ReactiveProperty<InitiativeTrackerUnitVM>();

	private bool m_NeedUpdate;

	public bool SkipScroll;

	public BoolReactiveProperty ConsoleActive = new BoolReactiveProperty();

	public InitiativeTrackerVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		UpdateUnits();
		RoundCounter.Value = Game.Instance.TurnController.CombatRound;
		AddDisposable(RoundCounter.Subscribe(delegate
		{
			CreateRoundVM();
		}));
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(delegate
		{
			TryUpdateUnits();
		}));
	}

	private void CreateRoundVM()
	{
		if (RoundVM == null)
		{
			AddDisposable(RoundVM = new InitiativeTrackerUnitVM(RoundCounter.Value + 1));
		}
		else
		{
			RoundVM.Round.Value = RoundCounter.Value + 1;
		}
	}

	protected override void DisposeImplementation()
	{
		m_NeedUpdate = false;
		Units.ForEach(delegate(InitiativeTrackerUnitVM u)
		{
			u.Dispose();
		});
		Units.Clear();
		foreach (InitiativeTrackerSquadLeaderVM value in Dictionary.Values)
		{
			value.Dispose();
		}
		Dictionary.Clear();
		CanInterruptMovement.Value = false;
	}

	public void HandleUnitSpawned()
	{
		m_NeedUpdate = true;
	}

	public void HandleUnitDestroyed()
	{
		m_NeedUpdate = true;
	}

	public void HandleUnitDeath()
	{
		m_NeedUpdate = true;
	}

	public void HandleUnitJoinCombat()
	{
		m_NeedUpdate = true;
	}

	public void HandleUnitLeaveCombat()
	{
		m_NeedUpdate = true;
	}

	public void HandleObjectInGameChanged()
	{
		if (EventInvokerExtensions.BaseUnitEntity != null)
		{
			m_NeedUpdate = true;
		}
	}

	public void HandleUnitMount(BaseUnitEntity mount)
	{
		m_NeedUpdate = true;
	}

	public void HandleUnitDismount([CanBeNull] BaseUnitEntity mount)
	{
		m_NeedUpdate = true;
	}

	private void UpdateUnits()
	{
		if (!Game.Instance.TurnController.TurnBasedModeActive)
		{
			return;
		}
		TurnController turnController = Game.Instance.TurnController;
		MechanicEntity mechanicEntity = turnController.CurrentUnit ?? turnController.TurnOrder.CurrentRoundUnitsOrder.FirstOrDefault();
		if (mechanicEntity == null)
		{
			return;
		}
		bool isDirty = false;
		int count = Units.Count;
		int num = 0;
		List<InitiativeTrackerUnitVM> list = new List<InitiativeTrackerUnitVM>();
		InitiativeTrackerUnitVM initiativeTrackerUnitVM = EnsureUnit(mechanicEntity, num, isCurrent: true, ref isDirty);
		list.Add(initiativeTrackerUnitVM);
		CurrentUnit.Value = initiativeTrackerUnitVM;
		UnitSquad unitSquad = mechanicEntity as UnitSquad;
		foreach (MechanicEntity item in turnController.UnitsAndSquadsByInitiativeForCurrentTurn)
		{
			if (item != mechanicEntity && (unitSquad == null || (item != unitSquad.Leader && item != unitSquad.Units.FirstItem().ToBaseUnitEntity())) && CheckVisibiltyInTracker(item))
			{
				initiativeTrackerUnitVM = EnsureUnit(item, ++num, isCurrent: false, ref isDirty);
				list.Add(initiativeTrackerUnitVM);
			}
		}
		RoundIndex = num;
		foreach (MechanicEntity item2 in turnController.UnitsAndSquadsByInitiativeForNextTurn.Except(turnController.UnitsAndSquadsByInitiativeForCurrentTurn))
		{
			if (CheckVisibiltyInTracker(item2))
			{
				initiativeTrackerUnitVM = EnsureUnit(item2, ++num, isCurrent: false, ref isDirty);
				list.Add(initiativeTrackerUnitVM);
			}
		}
		if (num != count)
		{
			isDirty = true;
		}
		if (isDirty)
		{
			foreach (InitiativeTrackerUnitVM item3 in Units.Except(list).ToList())
			{
				Units.Remove(item3);
				item3.Dispose();
			}
			Units = Units.OrderBy((InitiativeTrackerUnitVM u) => u.OrderIndex.Value).ToList();
			foreach (InitiativeTrackerUnitVM unit in Units)
			{
				if (!unit.IsInSquad.Value)
				{
					continue;
				}
				if (Dictionary.ContainsKey(unit.SquadGroupIndex.Value))
				{
					Dictionary[unit.SquadGroupIndex.Value].AddToSquad(unit);
					if (unit.IsSquadLeader.Value)
					{
						Dictionary[unit.SquadGroupIndex.Value].SetSquadLeader(unit);
					}
				}
				else
				{
					Dictionary[unit.SquadGroupIndex.Value] = new InitiativeTrackerSquadLeaderVM(unit.Unit, ++num, isCurrent: false);
					if (unit.IsSquadLeader.Value)
					{
						Dictionary[unit.SquadGroupIndex.Value].SetSquadLeader(unit);
					}
				}
			}
			UnitsUpdated.Execute();
		}
		m_NeedUpdate = false;
	}

	private bool CheckVisibiltyInTracker(MechanicEntity entity)
	{
		if (!(entity is UnitSquad))
		{
			if (!entity.IsVisibleForPlayer && (!(entity is BaseUnitEntity baseUnitEntity) || !baseUnitEntity.IsSummoned()))
			{
				return entity is InitiativePlaceholderEntity;
			}
			return true;
		}
		return false;
	}

	private InitiativeTrackerUnitVM EnsureUnit(MechanicEntity unit, int index, bool isCurrent, ref bool isDirty)
	{
		int num = Units.FindIndex((InitiativeTrackerUnitVM v) => v.Unit == unit);
		InitiativeTrackerUnitVM initiativeTrackerUnitVM = ((num >= 0) ? Units[num] : null);
		if (initiativeTrackerUnitVM == null)
		{
			InitiativeTrackerUnitVM initiativeTrackerUnitVM2 = new InitiativeTrackerUnitVM(unit, index, isCurrent);
			Units.Add(initiativeTrackerUnitVM2);
			isDirty = true;
			return initiativeTrackerUnitVM2;
		}
		if (initiativeTrackerUnitVM.OrderIndex.Value != index || initiativeTrackerUnitVM.IsCurrent.Value != isCurrent)
		{
			initiativeTrackerUnitVM.UpdateData(index, isCurrent);
			isDirty = true;
		}
		else
		{
			initiativeTrackerUnitVM.UpdateData();
		}
		return initiativeTrackerUnitVM;
	}

	private void TryUpdateUnits()
	{
		if (m_NeedUpdate)
		{
			UpdateUnits();
		}
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		if (isHover)
		{
			BaseUnitEntity baseUnitEntity = unitEntityView.EntityData.ToBaseUnitEntity();
			if (baseUnitEntity.IsInSquad)
			{
				PartSquad squadOptional = baseUnitEntity.GetSquadOptional();
				BaseUnitEntity leader = squadOptional?.Leader ?? ((squadOptional != null) ? squadOptional.Units.FirstOrDefault().ToBaseUnitEntity() : null);
				if (leader != null)
				{
					SquadLeaderUnit.Value = Units.LastOrDefault((InitiativeTrackerUnitVM unit) => unit.Unit == leader);
				}
			}
			HoveredUnit.Value = Units.LastOrDefault((InitiativeTrackerUnitVM unit) => unit.Unit?.View == unitEntityView);
			{
				foreach (InitiativeTrackerUnitVM unit in Units)
				{
					unit.UnitState?.HandleHoverChange(unitEntityView, unit.Unit?.View == unitEntityView);
				}
				return;
			}
		}
		HoveredUnit.Value = null;
		foreach (InitiativeTrackerUnitVM unit2 in Units)
		{
			unit2.UnitState?.HandleHoverChange(unitEntityView, isHover: false);
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_NeedUpdate = isTurnBased;
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		m_NeedUpdate = true;
		RoundCounter.Value = Game.Instance.TurnController.CombatRound;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		m_NeedUpdate = true;
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		m_NeedUpdate = true;
	}

	public void HandleInitiativeChanged()
	{
		m_NeedUpdate = true;
	}

	public void OnEntityBecameVisible()
	{
		if (EventInvokerExtensions.BaseUnitEntity != null)
		{
			m_NeedUpdate = true;
		}
	}

	public void OnEntityBecameInvisible()
	{
		if (EventInvokerExtensions.BaseUnitEntity != null)
		{
			m_NeedUpdate = true;
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		m_NeedUpdate = true;
	}

	public void HandleShowChange()
	{
		m_NeedUpdate = true;
		SkipScroll = true;
	}
}
