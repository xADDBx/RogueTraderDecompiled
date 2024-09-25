using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class ShipStatsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISelectionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public readonly ReactiveProperty<string> Speed = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> Inertia = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> Evasion = new ReactiveProperty<string>(string.Empty);

	private bool m_Dirty = true;

	public ShipStatsVM()
	{
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private void OnUpdateHandler()
	{
		UpdateStats();
	}

	private void UpdateStats()
	{
		if (m_Dirty)
		{
			StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
			Speed.Value = playerShip.CombatState.WarhammerInitialAPBlue.ModifiedValue.ToString();
			Inertia.Value = (6 - playerShip.Stats.GetStat(StatType.Inertia)?.ModifiedValue).ToString();
		}
	}

	public void OnUnitSelectionAdd(bool single, bool ask)
	{
		m_Dirty = true;
	}

	public void OnUnitSelectionRemove()
	{
		m_Dirty = true;
	}
}
