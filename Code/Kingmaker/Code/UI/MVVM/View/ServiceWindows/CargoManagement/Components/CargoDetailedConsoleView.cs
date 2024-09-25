using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoDetailedConsoleView : CargoDetailedBaseView, IConsoleEntityProxy, IConsoleEntity
{
	public ReactiveProperty<ItemSlotVM> Selected = new ReactiveProperty<ItemSlotVM>();

	private List<IConsoleNavigationEntity> m_Entities = new List<IConsoleNavigationEntity>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	public IConsoleEntity ConsoleEntityProxy => m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		OnCargoUpdate();
		m_NavigationBehaviour = base.ItemSlotsGroup.GetNavigation();
	}
}
