using System;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipCoverBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IRoundStartHandler, IUnitCommandEndHandler, IUnitCommandStartHandler, IVirtualPositionUIHandler
{
	public readonly UnitState UnitState;

	public readonly ReactiveProperty<LosCalculations.CoverType> CoverType = new ReactiveProperty<LosCalculations.CoverType>(LosCalculations.CoverType.None);

	public readonly ReactiveProperty<bool> NeedCover = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<float> CoverChance = new ReactiveProperty<float>(0f);

	private MechanicEntity Unit => UnitState.Unit.MechanicEntity;

	public OvertipCoverBlockVM(UnitState unitState)
	{
		UnitState = unitState;
		AddDisposable(EventBus.Subscribe(this));
		UpdateCover();
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateCover();
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		UpdateCover();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateCover();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateCover();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateCover();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		UpdateCover();
	}

	public void HandleVirtualPositionChanged(Vector3? position)
	{
		UpdateCover();
	}

	private void UpdateCover()
	{
		if (!Game.Instance.TurnController.TurnBasedModeActive)
		{
			NeedCover.Value = false;
		}
		else
		{
			if (UnitState.IsDead.Value)
			{
				return;
			}
			if (Unit == null)
			{
				UberDebug.LogError("UnitState: Unit is null");
				return;
			}
			bool isPlayerTurn = Game.Instance.TurnController.IsPlayerTurn;
			MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
			VirtualPositionController virtualPositionController = Game.Instance.VirtualPositionController;
			if (isPlayerTurn && currentUnit != null && virtualPositionController != null && Unit.IsPlayerEnemy)
			{
				Vector3 bestShootingPosition = LosCalculations.GetBestShootingPosition(virtualPositionController.GetDesiredPosition(currentUnit), currentUnit.SizeRect, Unit.Position, Unit.SizeRect);
				CoverType.Value = LosCalculations.GetWarhammerLos(bestShootingPosition, currentUnit.SizeRect, Unit);
				NeedCover.Value = true;
			}
			else
			{
				CoverChance.Value = 0f;
				NeedCover.Value = false;
			}
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateCover();
	}
}
