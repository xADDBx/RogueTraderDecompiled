using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;

public class InitiativeTrackerSquadLeaderVM : InitiativeTrackerUnitVM
{
	private List<InitiativeTrackerUnitVM> m_SquadUnits = new List<InitiativeTrackerUnitVM>();

	private InitiativeTrackerUnitVM m_Leader;

	public InitiativeTrackerSquadLeaderVM(int round)
		: base(round)
	{
	}

	public InitiativeTrackerSquadLeaderVM(MechanicEntity unit, int index, bool isCurrent)
		: base(unit, index, isCurrent)
	{
	}

	public void SetSquadLeader(InitiativeTrackerUnitVM leader)
	{
		m_Leader = leader;
		AddDisposable(m_Leader.NeedToShow.Skip(1).Subscribe(delegate(bool x)
		{
			HandleShowingSquadChange(x);
		}));
	}

	public void AddToSquad(InitiativeTrackerUnitVM unit)
	{
		m_SquadUnits.Add(unit);
	}

	private void HandleShowingSquadChange(bool val)
	{
		foreach (InitiativeTrackerUnitVM squadUnit in m_SquadUnits)
		{
			squadUnit.NeedToShow.Value = val;
		}
		EventBus.RaiseEvent(delegate(IInitiativeTrackerShowGroup h)
		{
			h.HandleShowChange();
		});
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
	}
}
