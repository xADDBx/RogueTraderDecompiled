using Kingmaker.EntitySystem.Entities;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;

public class InitiativeTrackerUnitVM : SurfaceCombatUnitVM
{
	public readonly IntReactiveProperty Round;

	public readonly IntReactiveProperty OrderIndex;

	public InitiativeTrackerUnitVM(int round)
	{
		AddDisposable(Round = new IntReactiveProperty(round));
	}

	public InitiativeTrackerUnitVM(MechanicEntity unit, int index, bool isCurrent)
		: base(unit, isCurrent)
	{
		AddDisposable(OrderIndex = new IntReactiveProperty(index));
	}

	public void UpdateData(int index, bool isCurrent)
	{
		if (base.Unit != null)
		{
			OrderIndex.Value = index;
			IsCurrent.Value = isCurrent;
			UpdateData();
		}
	}
}
