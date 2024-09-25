using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;

public class ProgressionBreadcrumbsItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly UnitProgressionWindowState ProgressionState;

	public readonly string Text;

	public readonly bool IsCurrent;

	private readonly Action<UnitProgressionWindowState> m_SetStateAction;

	public ProgressionBreadcrumbsItemVM(UnitProgressionWindowState progressionState, string text, bool isCurrent, Action<UnitProgressionWindowState> setStateAction)
	{
		ProgressionState = progressionState;
		Text = text;
		IsCurrent = isCurrent;
		m_SetStateAction = setStateAction;
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleClick()
	{
		m_SetStateAction?.Invoke(ProgressionState);
	}
}
