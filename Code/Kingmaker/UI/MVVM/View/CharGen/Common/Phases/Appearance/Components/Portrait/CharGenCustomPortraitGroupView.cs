using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public class CharGenCustomPortraitGroupView : ViewBase<CharGenPortraitGroupVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharGenPortraitSelectorItemView m_Prefab;

	[SerializeField]
	private CharGenCustomPortraitCreatorItemView m_CreatorItemPrefab;

	[SerializeField]
	private int m_ItemsInRow;

	private GridConsoleNavigationBehaviour m_Navigation;

	public IConsoleEntity ConsoleEntityProxy => m_Navigation;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.PortraitCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
		DrawEntities();
	}

	private void DrawEntities()
	{
		AddDisposable(m_WidgetList.DrawMultiEntries(base.ViewModel.PortraitCollection, new List<IWidgetView> { m_Prefab, m_CreatorItemPrefab }));
		UpdateNavigation();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void UpdateNavigation()
	{
		if (m_Navigation == null)
		{
			AddDisposable(m_Navigation = new GridConsoleNavigationBehaviour());
		}
		m_Navigation.Clear();
		m_Navigation.SetEntitiesGrid(m_WidgetList.Entries.Cast<IConsoleEntity>().ToList(), m_ItemsInRow);
	}

	public void FocusOnSelectedEntityOrFirst()
	{
		IConsoleNavigationEntity selectedEntity = GetSelectedEntity();
		if (selectedEntity == null)
		{
			m_Navigation.FocusOnFirstValidEntity();
		}
		else
		{
			m_Navigation.FocusOnEntityManual(selectedEntity);
		}
	}

	private IConsoleNavigationEntity GetSelectedEntity()
	{
		CharGenPortraitSelectorItemView charGenPortraitSelectorItemView = m_WidgetList.Entries?.Where((IWidgetView e) => e is CharGenPortraitSelectorItemView).Cast<CharGenPortraitSelectorItemView>().FirstOrDefault((CharGenPortraitSelectorItemView i) => i.IsSelected);
		if (!(charGenPortraitSelectorItemView == null))
		{
			return charGenPortraitSelectorItemView;
		}
		return m_WidgetList.Entries?.FirstOrDefault() as IConsoleNavigationEntity;
	}
}
