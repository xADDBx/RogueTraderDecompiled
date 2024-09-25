using Kingmaker.UI.MVVM.VM.CombatLog;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CombatLog;

public class CombatLogSeparatorView : VirtualListElementViewBase<CombatLogSeparatorVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutElementSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutElementSettings;

	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
	}

	public bool IsValid()
	{
		return false;
	}
}
