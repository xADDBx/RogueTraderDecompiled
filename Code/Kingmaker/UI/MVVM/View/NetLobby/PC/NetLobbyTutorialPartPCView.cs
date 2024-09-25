using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.PC;

public class NetLobbyTutorialPartPCView : NetLobbyTutorialPartBaseView
{
	[SerializeField]
	private OwlcatButton m_ContinueButton;

	[SerializeField]
	private TextMeshProUGUI m_ContinueButtonLabel;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ContinueButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			ShowBlock();
		}));
		m_ContinueButtonLabel.text = UIStrings.Instance.CommonTexts.Accept;
	}
}
