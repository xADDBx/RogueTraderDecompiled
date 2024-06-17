using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipComponentItemSlotConsoleView : ShipComponentItemSlotBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(base.OnClick));
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}
}
