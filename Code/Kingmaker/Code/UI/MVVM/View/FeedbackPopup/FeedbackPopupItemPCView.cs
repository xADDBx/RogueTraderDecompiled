using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.FeedbackPopup;

public class FeedbackPopupItemPCView : ViewBase<FeedbackPopupItemVM>, IWidgetView
{
	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Image m_Icon;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Label.text = base.ViewModel.Label;
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.HandleClick();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as FeedbackPopupItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is FeedbackPopupItemVM;
	}
}
