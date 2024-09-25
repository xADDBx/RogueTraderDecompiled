using System;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipPointBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler
{
	public readonly UnitState UnitState;

	private IDisposable m_UpdateDispatcher;

	public readonly ReactiveProperty<bool> NeedToShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<float> MovePoints = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> ActionPoints = new ReactiveProperty<float>(0f);

	private MechanicEntity Unit => UnitState.Unit.MechanicEntity;

	private MechanicEntityUIWrapper UnitUIWrapper => UnitState.Unit;

	public OvertipPointBlockVM(UnitState unitState)
	{
		UnitState = unitState;
		if (Unit.IsInPlayerParty)
		{
			AddDisposable(EventBus.Subscribe(this));
			if (Game.Instance.TurnController.TurnBasedModeActive)
			{
				OnNewUnitTurnCheck(Game.Instance.TurnController.CurrentUnit);
			}
		}
	}

	protected override void DisposeImplementation()
	{
		m_UpdateDispatcher?.Dispose();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		OnNewUnitTurnCheck(EventInvokerExtensions.MechanicEntity);
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		OnNewUnitTurnCheck(EventInvokerExtensions.MechanicEntity);
	}

	private void OnNewUnitTurnCheck(MechanicEntity unit)
	{
		m_UpdateDispatcher?.Dispose();
		if (!Game.Instance.TurnController.TurnBasedModeActive)
		{
			NeedToShow.Value = false;
			return;
		}
		if (Unit == unit)
		{
			UpdateProperties();
			m_UpdateDispatcher = MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
			{
				UpdateProperties();
			});
		}
		NeedToShow.Value = Unit == unit;
	}

	private void UpdateProperties()
	{
		MovePoints.Value = UnitUIWrapper.CombatState.ActionPointsBlue;
		ActionPoints.Value = UnitUIWrapper.CombatState.ActionPointsYellow;
	}
}
