using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class StatusEffectConsoleView : StatusEffectBaseView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	protected OwlcatMultiSelectable m_Selectable;

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Selectable.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}
}
