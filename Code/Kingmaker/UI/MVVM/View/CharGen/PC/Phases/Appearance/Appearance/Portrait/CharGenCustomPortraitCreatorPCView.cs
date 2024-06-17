using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.PC.Phases.Appearance.Appearance.Portrait;

public class CharGenCustomPortraitCreatorPCView : CharGenCustomPortraitCreatorView
{
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private TextMeshProUGUI m_CloseButtonLabel;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClose();
		}));
		m_CloseButtonLabel.text = UIStrings.Instance.CommonTexts.Cancel;
	}
}
