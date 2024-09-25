using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Classes;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Classes;

public class CharInfoClassesListPCView : ViewBase<CharInfoClassesListVM>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoClassEntryPCView m_ClassEntry;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.ClassVMs, m_ClassEntry));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
