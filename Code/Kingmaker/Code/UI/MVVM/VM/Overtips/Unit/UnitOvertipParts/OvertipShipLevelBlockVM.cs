using System;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipShipLevelBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly UnitState UnitState;

	public readonly ReactiveProperty<string> Level = new ReactiveProperty<string>(string.Empty);

	private MechanicEntity Unit => UnitState.Unit.MechanicEntity;

	public OvertipShipLevelBlockVM(UnitState unitState)
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		UnitState = unitState;
		Level.Value = UIUtility.ArabicToRoman(playerShip.Progression.CharacterLevel);
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}
}
