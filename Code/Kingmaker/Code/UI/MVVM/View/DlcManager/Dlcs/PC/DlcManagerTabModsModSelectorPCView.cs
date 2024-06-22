using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.PC;
using Kingmaker.UI.MVVM.View.DlcManager.Dlcs.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.PC;

public class DlcManagerTabModsModSelectorPCView : DlcManagerTabModsModSelectorBaseView
{
	[Header("Console Part")]
	[SerializeField]
	[UsedImplicitly]
	private DlcManagerModEntityPCView m_ItemPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
	}
}
