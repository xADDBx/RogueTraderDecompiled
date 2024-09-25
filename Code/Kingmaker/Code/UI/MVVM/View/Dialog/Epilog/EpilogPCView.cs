using System.Linq;
using Kingmaker.Localization;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Epilog;

public class EpilogPCView : EpilogBaseView
{
	[SerializeField]
	protected OwlcatButton m_ContinueButton;

	[SerializeField]
	protected TextMeshProUGUI m_ContinueButtonTitle;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(Game.Instance.Keyboard.Bind("NextOrEnd", Confirm));
		AddDisposable(m_ContinueButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			Confirm();
		}));
	}

	protected override void OnAnswersChanged()
	{
		LocalizedString localizedString = base.ViewModel.Answers.Value?.FirstOrDefault()?.Answer.Value?.Text;
		string text = ((localizedString != null) ? ((string)localizedString) : string.Empty).Replace(".", string.Empty);
		m_ContinueButtonTitle.text = text;
	}
}
