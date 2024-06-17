using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorReputationForItemConsoleView : VendorReputationForItemView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private OwlcatMultiButton m_FocusButton;

	public void SetFocus(bool value)
	{
		m_FocusButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_FocusButton.IsValid();
	}
}
