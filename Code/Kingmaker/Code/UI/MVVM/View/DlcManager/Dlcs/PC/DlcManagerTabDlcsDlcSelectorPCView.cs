using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.PC;

public class DlcManagerTabDlcsDlcSelectorPCView : DlcManagerTabDlcsDlcSelectorBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerDlcEntityPCView m_ItemPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
	}

	public void UpdateDlcEntities()
	{
		List<DlcManagerDlcEntityPCView> list = m_WidgetList.Or(null)?.Entries?.OfType<DlcManagerDlcEntityPCView>().ToList();
		if (list != null && list.Any())
		{
			list.ForEach(delegate(DlcManagerDlcEntityPCView e)
			{
				e.UpdateGrayScale();
			});
		}
	}
}
