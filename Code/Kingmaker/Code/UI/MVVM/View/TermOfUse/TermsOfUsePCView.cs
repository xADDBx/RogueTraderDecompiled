using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.TermOfUse.Base;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.TermOfUse;

public class TermsOfUsePCView : TermsOfUseBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_OkLabel;

	[SerializeField]
	protected OwlcatButton m_ButtonOk;

	[SerializeField]
	protected OwlcatButton m_ButtonClose;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_OkLabel.text = base.ViewModel.TermsOfUseTexts.AcceptBtn;
		m_ButtonOk.transform.parent.gameObject.SetActive(TermsOfUseBaseView.IsShowFirstTime);
		if (!TermsOfUseBaseView.IsShowFirstTime)
		{
			AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
			{
				base.ViewModel.TermsOfUseClose();
			}));
		}
		AddDisposable(m_ButtonOk.OnLeftClickAsObservable().Subscribe(delegate
		{
			if (TermsOfUseBaseView.IsShowFirstTime)
			{
				base.ViewModel.TermsOfUseAccept();
			}
			else
			{
				base.ViewModel.TermsOfUseClose();
			}
		}));
		AddDisposable(m_ButtonClose.OnLeftClickAsObservable().Subscribe(delegate
		{
			if (TermsOfUseBaseView.IsShowFirstTime)
			{
				base.ViewModel.TermsOfUseDecline();
			}
			else
			{
				base.ViewModel.TermsOfUseClose();
			}
		}));
	}
}
