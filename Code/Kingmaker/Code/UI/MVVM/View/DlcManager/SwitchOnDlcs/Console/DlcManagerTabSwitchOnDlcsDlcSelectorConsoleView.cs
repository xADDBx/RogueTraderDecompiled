using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Console;

public class DlcManagerTabSwitchOnDlcsDlcSelectorConsoleView : DlcManagerTabSwitchOnDlcsDlcSelectorBaseView
{
	[Header("Console Part")]
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerSwitchOnDlcEntityConsoleView m_ItemPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (m_WidgetList.Entries == null || !m_WidgetList.Entries.Any())
		{
			return list;
		}
		list.AddRange(m_WidgetList.Entries.OfType<DlcManagerSwitchOnDlcEntityConsoleView>());
		return list;
	}
}
