using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class JournalNavigationGroupConsoleView : JournalNavigationGroupBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ExpandableCollapseMultiButtonConsole m_ExpandableCollapseMultiButton;

	[SerializeField]
	[UsedImplicitly]
	private JournalNavigationGroupElementConsoleView NavigationGroupElementViewPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		DrawEntities();
		m_ExpandableCollapseMultiButton.SetValue(!base.ViewModel.IsCollapse, isImmediately: true);
		AddDisposable(m_ExpandableCollapseMultiButton.IsOn.Subscribe(delegate(bool value)
		{
			base.ViewModel.IsCollapse = !value;
		}));
	}

	private void DrawEntities()
	{
		JournalQuestVM[] vmCollection = (base.ShowCompletedQuests ? base.ViewModel.Quests.ToArray() : base.ViewModel.Quests.Where((JournalQuestVM q) => q.IsActive).ToArray());
		base.WidgetList.DrawEntries(vmCollection, NavigationGroupElementViewPrefab);
	}

	public List<IConsoleEntity> GetSelectableEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		list.Add(m_MultiButton);
		foreach (IWidgetView entry in m_WidgetList.Entries)
		{
			if (entry is IConsoleEntity item)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
