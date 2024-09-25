using System;
using Kingmaker.GameModes;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipVoidshipHealthVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<int> MaxShipHealth = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentShipHealth = new ReactiveProperty<int>();

	public OvertipVoidshipHealthVM()
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem && Game.Instance.CurrentMode != GameModeType.SpaceCombat)
		{
			AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
			{
				OnUpdate();
			}));
		}
	}

	private void OnUpdate()
	{
		PartHealth partHealth = Game.Instance?.Player?.PlayerShip?.Health;
		if (partHealth != null)
		{
			MaxShipHealth.Value = partHealth.MaxHitPoints;
			CurrentShipHealth.Value = partHealth.HitPointsLeft;
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
