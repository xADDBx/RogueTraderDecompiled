using System;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.EntitySystem.Entities;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Party;

public class UnitBarkPartVM : BaseBarkVM
{
	public readonly ReactiveProperty<bool> IsUnitOnScreen = new ReactiveProperty<bool>();

	private BaseUnitEntity m_Unit;

	private IDisposable m_UnitVisibility;

	protected override void DisposeImplementation()
	{
		m_Unit = null;
		IsUnitOnScreen.Value = false;
	}

	public void SetUnitData(BaseUnitEntity unit)
	{
		m_Unit = unit;
		m_UnitVisibility?.Dispose();
		m_UnitVisibility = null;
		if (unit != null)
		{
			AddDisposable(UnitStatesHolderVM.Instance.GetOrCreateUnitState(unit).IsVisibleForPlayer.Subscribe(delegate(bool val)
			{
				IsUnitOnScreen.Value = val;
			}));
		}
	}
}
