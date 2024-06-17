using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorTransitionWindowPCView : VendorTransitionWindowView
{
	[SerializeField]
	protected OwlcatButton m_AcceptButton;

	[SerializeField]
	private TextMeshProUGUI m_AcceptButtonLabel;

	[FormerlySerializedAs("m_СancelButton")]
	[SerializeField]
	protected OwlcatButton m_CancelButton;

	[SerializeField]
	private TextMeshProUGUI m_СancelButtonLabel;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_СancelButtonLabel.text = UIStrings.Instance.CommonTexts.Cancel;
		m_AcceptButtonLabel.text = UIStrings.Instance.CommonTexts.Accept;
		AddDisposable(m_AcceptButton.OnLeftClickAsObservable().Subscribe(base.Deal));
		AddDisposable(m_CancelButton.OnLeftClickAsObservable().Subscribe(Close));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(Close));
	}
}
