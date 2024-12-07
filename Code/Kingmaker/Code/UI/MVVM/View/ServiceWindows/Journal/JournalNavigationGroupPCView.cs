using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;

public class JournalNavigationGroupPCView : JournalNavigationGroupBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ExpandableCollapseMultiButtonPC m_ExpandableCollapseMultiButton;

	[SerializeField]
	[UsedImplicitly]
	private JournalNavigationGroupElementPCView NavigationGroupElementViewPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		DrawEntities();
		DelayedInvoker.InvokeInFrames(delegate
		{
			if (base.ViewModel != null && !(m_ExpandableCollapseMultiButton == null))
			{
				m_ExpandableCollapseMultiButton.SetValue(!base.ViewModel.IsCollapse, isImmediately: true);
				AddDisposable(m_ExpandableCollapseMultiButton.IsOn.Subscribe(delegate(bool value)
				{
					base.ViewModel.IsCollapse = !value;
				}));
			}
		}, 3);
	}

	private void DrawEntities()
	{
		if (base.ViewModel != null)
		{
			JournalQuestVM[] vmCollection = (base.ShowCompletedQuests ? base.ViewModel.Quests.ToArray() : base.ViewModel.Quests.Where((JournalQuestVM q) => q.IsActive).ToArray());
			base.WidgetList.DrawEntries(vmCollection, NavigationGroupElementViewPrefab);
		}
	}
}
