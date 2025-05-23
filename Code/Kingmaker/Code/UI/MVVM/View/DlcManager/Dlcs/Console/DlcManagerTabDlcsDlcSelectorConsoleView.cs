using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Base;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;
using Kingmaker.DLC;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Console;

public class DlcManagerTabDlcsDlcSelectorConsoleView : DlcManagerTabDlcsDlcSelectorBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerDlcEntityConsoleView m_ItemPrefab;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveAdd().Subscribe(delegate
		{
			DrawEntries();
		}));
		DrawEntries();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_WidgetList.Clear();
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return m_NavigationBehaviour;
	}

	private void DrawEntries()
	{
		BlueprintDlc selectedDlc = base.ViewModel.SelectedEntity.Value?.BlueprintDlc;
		m_WidgetList.Clear();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		}
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddColumn(m_WidgetList.Entries.OfType<DlcManagerDlcEntityConsoleView>().ToList());
		DlcManagerDlcEntityConsoleView entity = m_WidgetList.Entries.OfType<DlcManagerDlcEntityConsoleView>().FirstOrDefault((DlcManagerDlcEntityConsoleView e) => (e.GetViewModel() as DlcManagerDlcEntityVM)?.BlueprintDlc == selectedDlc);
		m_NavigationBehaviour.FocusOnEntityManual(entity);
	}

	public void UpdateDlcEntities()
	{
		List<DlcManagerDlcEntityConsoleView> list = m_WidgetList.Or(null)?.Entries?.OfType<DlcManagerDlcEntityConsoleView>().ToList();
		if (list != null && list.Any())
		{
			list.ForEach(delegate(DlcManagerDlcEntityConsoleView e)
			{
				e.UpdateGrayScale();
			});
		}
	}
}
