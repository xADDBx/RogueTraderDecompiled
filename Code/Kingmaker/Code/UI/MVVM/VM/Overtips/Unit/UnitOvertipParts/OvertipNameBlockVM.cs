using System;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipNameBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly UnitState UnitState;

	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>(string.Empty);

	private MechanicEntity Unit => UnitState.Unit.MechanicEntity;

	private MechanicEntityUIWrapper UnitUIWrapper => UnitState.Unit;

	public OvertipNameBlockVM(UnitState unitState)
	{
		UnitState = unitState;
		Name.Value = UnitUIWrapper.Name;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}
}
