using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.Pause;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Pause.PC;

public class PauseNotificationPCView : PauseNotificationBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_UnpauseButton;

	[SerializeField]
	private TextMeshProUGUI m_UnpauseText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_UnpauseText.text = UIStrings.Instance.CommonTexts.Unpause;
		AddDisposable(m_UnpauseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Unpause();
		}));
	}
}
