using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Base;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Console;

public class DlcManagerTabDlcsDlcSelectorConsoleView : DlcManagerTabDlcsDlcSelectorBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerDlcEntityConsoleView m_ItemPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (m_WidgetList.Entries == null)
		{
			return list;
		}
		list.AddRange(m_WidgetList.Entries.OfType<DlcManagerDlcEntityConsoleView>());
		return list;
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
