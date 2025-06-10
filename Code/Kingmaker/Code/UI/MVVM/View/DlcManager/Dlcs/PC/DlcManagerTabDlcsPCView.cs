using System.Linq;
using Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.PC;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.PC;

public class DlcManagerTabDlcsPCView : DlcManagerTabDlcsBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private DlcManagerTabDlcsDlcSelectorPCView m_DlcSelectorPCView;

	[SerializeField]
	private CustomUIVideoPlayerPCView m_CustomUIVideoPlayerPCView;

	[SerializeField]
	private float m_DefaultFontDlcDescriptionPCSize = 25f;

	public override void Initialize()
	{
		base.Initialize();
		if (!IsInit)
		{
			m_CustomUIVideoPlayerPCView.Initialize();
			IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_CustomUIVideoPlayerPCView.Bind(base.ViewModel.CustomUIVideoPlayerVM);
		base.BindViewImplementation();
		m_DlcSelectorPCView.Bind(base.ViewModel.SelectionGroup);
		AddDisposable(m_PurchaseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowInStore();
		}));
	}

	protected override void ShowHideVideoImpl(bool state)
	{
		base.ShowHideVideoImpl(state);
		m_CustomUIVideoPlayerPCView.gameObject.SetActive(state);
	}

	protected override void UpdateDlcEntitiesImpl()
	{
		base.UpdateDlcEntitiesImpl();
		m_DlcSelectorPCView.UpdateDlcEntities();
		base.ViewModel.SelectedEntity.SetValueAndForceNotify(base.ViewModel.SelectionGroup.EntitiesCollection.FirstOrDefault());
		base.ViewModel.SelectedEntity.Value?.IsSelected.SetValueAndForceNotify(value: true);
	}

	protected override void SetTextFontSize(float multiplier)
	{
		base.SetTextFontSize(multiplier);
		m_DlcDescription.fontSize = m_DefaultFontDlcDescriptionPCSize * multiplier;
	}
}
