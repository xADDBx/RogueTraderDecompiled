using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil.Console;

public class VeilThicknessConsoleView : VeilThicknessView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip;
	}
}
