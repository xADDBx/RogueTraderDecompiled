using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.BackgroundBase;

public class CharGenBackgroundBasePhaseSelectorView : ViewBase<SelectionGroupRadioVM<CharGenBackgroundBaseItemVM>>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharGenBackgroundBasePhaseSelectorItemView<CharGenBackgroundBaseItemVM> m_ItemViewPrefab;

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
		return m_WidgetList.Entries.Cast<CharGenBackgroundBasePhaseSelectorItemView<CharGenBackgroundBaseItemVM>>().FirstOrDefault((CharGenBackgroundBasePhaseSelectorItemView<CharGenBackgroundBaseItemVM> i) => (i.GetViewModel() as CharGenBackgroundBaseItemVM)?.IsSelected.Value ?? false);
	}
}
