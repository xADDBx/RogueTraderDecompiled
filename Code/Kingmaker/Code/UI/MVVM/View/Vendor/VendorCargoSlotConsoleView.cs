using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorCargoSlotConsoleView : VendorCargoSlotView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_MainButton.OnFocusAsObservable().Subscribe(delegate
		{
			SetNotNewState();
		}));
	}

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
		return true;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.HandleClick();
		base.ViewModel.HandleCheck();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
