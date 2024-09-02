using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.PC;

public class DlcManagerTabSwitchOnDlcsDlcSelectorPCView : DlcManagerTabSwitchOnDlcsDlcSelectorBaseView
{
	[Header("PC Part")]
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerSwitchOnDlcEntityPCView m_ItemPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
	}
}
