using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Menu;

public class ServiceWindowsMenuEntityVM : SelectionGroupEntityVM
{
	public ServiceWindowsType ServiceWindowsType;

	public ServiceWindowsMenuEntityVM(ServiceWindowsType type)
		: base(allowSwitchOff: false)
	{
		ServiceWindowsType = type;
	}

	public void SetAvailable(bool available)
	{
		SetAvailableState(available);
	}

	protected override void DoSelectMe()
	{
	}
}
