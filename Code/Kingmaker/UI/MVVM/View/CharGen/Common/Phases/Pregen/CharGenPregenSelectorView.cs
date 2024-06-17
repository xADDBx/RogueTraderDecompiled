using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Pregen;

public class CharGenPregenSelectorView : ViewBase<SelectionGroupRadioVM<CharGenPregenSelectorItemVM>>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharGenPregenSelectorItemView m_ItemViewPrefab;

	protected override void BindViewImplementation()
	{
		DrawEntities();
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemViewPrefab, strictMatching: true);
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenPregenSelectorItemView>().FirstOrDefault((CharGenPregenSelectorItemView i) => (i.GetViewModel() as CharGenPregenSelectorItemVM)?.IsSelected.Value ?? false);
	}
}
