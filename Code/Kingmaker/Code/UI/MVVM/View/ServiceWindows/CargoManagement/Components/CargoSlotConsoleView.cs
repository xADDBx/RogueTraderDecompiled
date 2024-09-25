using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoSlotConsoleView : CargoSlotBaseView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_MainButton.IsValid();
	}

	public bool CanConfirmClick()
	{
		return m_MainButton.enabled;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.HandleClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public void Highlight()
	{
		SetFocus(value: true);
		DelayedInvoker.InvokeInTime(delegate
		{
			SetFocus(value: false);
		}, 1f);
	}
}
