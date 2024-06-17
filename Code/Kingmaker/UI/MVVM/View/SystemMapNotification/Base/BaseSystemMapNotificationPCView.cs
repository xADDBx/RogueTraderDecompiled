using Kingmaker.UI.MVVM.VM.SystemMapNotification;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMapNotification.Base;

public abstract class BaseSystemMapNotificationPCView<TViewModel> : BaseSystemMapNotificationView<TViewModel> where TViewModel : SystemMapNotificationBaseVM
{
	[SerializeField]
	protected OwlcatButton m_ActionButton;

	[SerializeField]
	protected OwlcatButton m_FullBodyButton;

	[SerializeField]
	protected OwlcatMultiButton m_CloseButton;

	[SerializeField]
	protected TextMeshProUGUI m_ActionButtonLabel;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ActionButton.OnLeftClickAsObservable().Subscribe(OnButtonClick));
		if (m_FullBodyButton != null)
		{
			AddDisposable(m_FullBodyButton.OnLeftClickAsObservable().Subscribe(OnButtonClick));
		}
		AddDisposable(m_CloseButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			Hide();
		}));
	}

	private void OnButtonClick()
	{
		OnButtonClickImpl();
	}

	protected virtual void OnButtonClickImpl()
	{
	}
}
