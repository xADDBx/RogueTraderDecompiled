using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorGenericSlotConsoleView : VendorGenericSlotView<ItemSlotConsoleView>, IConsoleNavigationEntity, IConsoleEntity
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotView.Bind(base.ViewModel);
	}

	public void SetFocus(bool value)
	{
		m_ItemSlotView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_ItemSlotView.IsValid();
	}
}
