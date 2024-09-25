using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;

public class EncyclopediaNavigationBaseView : ViewBase<EncyclopediaNavigationVM>
{
	[Header("Chapters Group Objects")]
	[SerializeField]
	private WidgetListMVVM m_ChaptersWidgetList;

	[SerializeField]
	private EncyclopediaNavigationChapterElementBaseView m_ChaptersNavigationViewPrefab;

	[Header("Pages Group Objects")]
	[SerializeField]
	private WidgetListMVVM m_PagesWidgetList;

	[SerializeField]
	private EncyclopediaNavigationElementBaseView m_PagesNavigationViewPrefab;

	[Header("Paper")]
	[SerializeField]
	private RectTransform m_PaperTransform;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRectChapters;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRectPages;

	protected override void BindViewImplementation()
	{
		DrawChapters();
		AddDisposable(base.ViewModel.SelectedChapter.Subscribe(DrawPages));
	}

	protected override void DestroyViewImplementation()
	{
		m_ChaptersWidgetList.Entries.ForEach(delegate(IWidgetView s)
		{
			s.BindWidgetVM(null);
		});
		m_ChaptersWidgetList.Clear();
		m_PagesWidgetList.Entries.ForEach(delegate(IWidgetView s)
		{
			s.BindWidgetVM(null);
		});
		m_PagesWidgetList.Clear();
	}

	private void DrawChapters()
	{
		EncyclopediaNavigationElementVM[] vmCollection = base.ViewModel.NavigationChapters.ToArray();
		AddDisposable(m_ChaptersWidgetList.DrawEntries(vmCollection, m_ChaptersNavigationViewPrefab));
		m_PaperTransform.SetSiblingIndex(0);
	}

	private void DrawPages(EncyclopediaNavigationElementVM groupVm)
	{
		m_PagesWidgetList.Clear();
		if (groupVm != null)
		{
			EncyclopediaNavigationElementVM[] vmCollection = groupVm.GetOrCreateChildsVM().ToArray();
			AddDisposable(m_PagesWidgetList.DrawEntries(vmCollection, m_PagesNavigationViewPrefab));
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_ScrollRectPages.ScrollToTop();
			}, 1);
		}
	}

	public void ScrollMenu(IConsoleEntity entity, bool isChapter)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			if (isChapter)
			{
				m_ScrollRectChapters.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
			}
			else
			{
				m_ScrollRectPages.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
			}
		}
	}

	public List<IConsoleEntity> GetNavigationEntities(bool isChapter)
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		List<IWidgetView> list2 = (isChapter ? m_ChaptersWidgetList.Entries : m_PagesWidgetList.Entries);
		if (list2 != null)
		{
			foreach (MonoBehaviour item2 in list2)
			{
				if (item2 is EncyclopediaNavigationElementBaseView item)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
