using System;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;

public class ShipCrewDollModuleVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ShipModuleType ModuleType;

	public readonly ReactiveProperty<ShipCrewModuleState> ModuleState = new ReactiveProperty<ShipCrewModuleState>(ShipCrewModuleState.FullyStaffed);

	private readonly Action m_ClickAction;

	public ShipCrewDollModuleVM(ShipModuleType moduleType, Action clickAction, StarshipEntity starshipEntity)
	{
		ModuleType = moduleType;
		ModuleState.Value = starshipEntity.Crew.GetReadOnlyCrewData(moduleType).GetState(includeInTransition: false);
		m_ClickAction = clickAction;
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleClick()
	{
		m_ClickAction?.Invoke();
	}
}
