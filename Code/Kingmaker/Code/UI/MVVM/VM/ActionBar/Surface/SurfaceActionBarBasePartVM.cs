using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;

public abstract class SurfaceActionBarBasePartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public EntityRef<BaseUnitEntity> Unit;

	public readonly ReactiveCommand UnitChanged = new ReactiveCommand();

	protected override void DisposeImplementation()
	{
		ClearSlots();
	}

	public void SetUnit(EntityRef<BaseUnitEntity> unit)
	{
		Unit = unit;
		if (unit != null)
		{
			OnUnitChanged();
			UnitChanged.Execute();
		}
		else
		{
			ClearSlots();
		}
	}

	protected abstract void OnUnitChanged();

	protected abstract void ClearSlots();
}
