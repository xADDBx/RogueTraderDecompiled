using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoDetailedZoneConsoleView : CargoDetailedZoneBaseView
{
	private bool m_InputAdded;

	public readonly ReactiveCommand OnHideSlot = new ReactiveCommand();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		foreach (CargoSlotVM cargoSlot in base.ViewModel.CargoSlots)
		{
			AddDisposable(cargoSlot.IsAvailable.Skip(1).Subscribe(HandleSlotChange));
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_InputAdded = false;
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		if (!m_InputAdded)
		{
			(SearchView as ItemsFilterSearchConsoleView)?.AddInput(inputLayer, enabledHints);
			m_InputAdded = true;
		}
	}

	public List<IDisposable> AddInputDisposable(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		List<IDisposable> list = new List<IDisposable>();
		if (!m_InputAdded)
		{
			list.Add((SearchView as ItemsFilterSearchConsoleView)?.AddInputDisposable(inputLayer, enabledHints));
			m_InputAdded = true;
		}
		return list;
	}

	public void HandleSlotChange(bool value)
	{
		if (!value)
		{
			OnHideSlot?.Execute();
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		return m_VirtualList.GetNavigationBehaviour();
	}
}
