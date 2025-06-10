using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostOfficerSelectionGroup : ViewBase<SelectionGroupRadioVM<PostOfficerVM>>
{
	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	protected PostOfficerView m_OfficerView;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		m_ScrollRect.ScrollToTop();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		DrawEntities();
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocus));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void OnFocus(IConsoleEntity entity)
	{
		PostOfficerView postOfficerView = entity as PostOfficerView;
		if (!(postOfficerView == null))
		{
			m_ScrollRect.EnsureVisible(postOfficerView.RectTransform);
		}
	}

	protected virtual void DrawEntities()
	{
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_OfficerView));
	}

	public ConsoleNavigationBehaviour GetNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesGrid(m_WidgetList.GetNavigationEntities(), 3);
		return m_NavigationBehaviour;
	}
}
