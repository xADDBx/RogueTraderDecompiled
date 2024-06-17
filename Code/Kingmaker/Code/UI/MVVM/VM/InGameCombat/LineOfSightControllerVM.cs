using System;
using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.InGameCombat;

public class LineOfSightControllerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IUnitCommandEndHandler, IUnitCommandStartHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IAreaHandler
{
	public readonly ReactiveCollection<LineOfSightVM> LinesVMs = new ReactiveCollection<LineOfSightVM>();

	public LineOfSightControllerVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		SetNewUnit();
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	private void RemoveLine(LineOfSightVM line)
	{
		line.Dispose();
		LinesVMs.Remove(line);
	}

	private LineOfSightVM GetLineByOwner(MechanicEntity owner)
	{
		return LinesVMs.FirstOrDefault((LineOfSightVM vm) => vm.Owner == owner);
	}

	private void Clear()
	{
		LinesVMs.ForEach(delegate(LineOfSightVM l)
		{
			l.Dispose();
		});
		LinesVMs.Clear();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			Clear();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			SetNewUnit();
		}
	}

	public void HandleUnitStartInterruptTurn()
	{
		SetNewUnit();
	}

	private void SetNewUnit()
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			Clear();
			MechanicEntity unit = Game.Instance.TurnController?.CurrentUnit;
			if (CheckUnit(unit))
			{
				SetUnitList(Game.Instance.TurnController.UnitsAndSquadsByInitiativeForCurrentTurn);
				SetUnitList(Game.Instance.TurnController.UnitsAndSquadsByInitiativeForNextTurn);
			}
		}
	}

	private void SetUnitList(IEnumerable<MechanicEntity> units)
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		foreach (MechanicEntity unit in units)
		{
			if (unit.IsPlayerEnemy && unit.IsVisibleForPlayer && !unit.IsDisposed)
			{
				LinesVMs.Add(new LineOfSightVM(currentUnit, unit));
			}
		}
	}

	private bool CheckUnit(MechanicEntity unit)
	{
		if (unit != null && !unit.IsPlayerEnemy)
		{
			return !(unit is UnitSquad);
		}
		return false;
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		AbstractUnitEntity executor = command.Executor;
		if ((executor == null || executor.IsInCombat) && TurnController.IsInTurnBasedCombat())
		{
			SetNewUnit();
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		AbstractUnitEntity executor = command.Executor;
		if ((executor == null || executor.IsInCombat) && TurnController.IsInTurnBasedCombat())
		{
			Clear();
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		if (!TurnController.IsInTurnBasedCombat())
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.LifeState.State == UnitLifeState.Dead)
		{
			LineOfSightVM lineByOwner = GetLineByOwner(baseUnitEntity);
			if (lineByOwner != null)
			{
				RemoveLine(lineByOwner);
			}
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		SetNewUnit();
	}

	public void OnAreaBeginUnloading()
	{
		Clear();
	}

	public void OnAreaDidLoad()
	{
		SetNewUnit();
	}
}
