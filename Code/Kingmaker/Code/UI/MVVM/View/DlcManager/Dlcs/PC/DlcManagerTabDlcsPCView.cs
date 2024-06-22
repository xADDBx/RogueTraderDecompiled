using System.Linq;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.PC;

public class DlcManagerTabDlcsPCView : DlcManagerTabDlcsBaseView
{
	[SerializeField]
	private DlcManagerTabDlcsDlcSelectorPCView m_DlcSelectorPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DlcSelectorPCView.Bind(base.ViewModel.SelectionGroup);
		AddDisposable(m_PurchaseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowInStore();
		}));
	}

	protected override void UpdateDlcEntitiesImpl()
	{
		base.UpdateDlcEntitiesImpl();
		m_DlcSelectorPCView.UpdateDlcEntities();
		base.ViewModel.SelectedEntity.SetValueAndForceNotify(base.ViewModel.SelectionGroup.EntitiesCollection.FirstOrDefault());
		base.ViewModel.SelectedEntity.Value.IsSelected.SetValueAndForceNotify(value: true);
	}
}
