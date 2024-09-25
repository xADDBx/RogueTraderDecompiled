using Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Console;

public class SaveSlotConsoleView : SaveSlotBaseView, IFunc01ClickHandler, IConsoleEntity, IFunc02ClickHandler, IConfirmClickHandler
{
	public bool CanFunc01Click()
	{
		return base.IsBinded;
	}

	public string GetFunc01ClickHint()
	{
		return string.Empty;
	}

	public void OnFunc01Click()
	{
		HandleFunc01Click();
	}

	public bool CanFunc02Click()
	{
		return base.IsBinded;
	}

	public string GetFunc02ClickHint()
	{
		return string.Empty;
	}

	public void OnFunc02Click()
	{
		base.ViewModel.ShowScreenshot();
	}

	protected virtual void HandleFunc01Click()
	{
		if (base.ViewModel.IsActuallySaved)
		{
			base.ViewModel.Delete();
		}
	}

	public new bool CanConfirmClick()
	{
		return true;
	}

	public new void OnConfirmClick()
	{
		base.ViewModel.SaveOrLoad();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelectedFromView(value);
		m_Button.CanConfirm = true;
	}
}
