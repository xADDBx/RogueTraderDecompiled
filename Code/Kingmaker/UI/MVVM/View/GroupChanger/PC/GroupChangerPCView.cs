using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.GroupChanger.PC;

public class GroupChangerPCView : GroupChangerBaseView
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_AcceptButton;

	[SerializeField]
	private TextMeshProUGUI m_AcceptLabel;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		bool flag = UINetUtility.IsControlMainCharacter();
		m_AcceptButton.Or(null)?.gameObject.SetActive(flag);
		if (flag)
		{
			AddDisposable(m_AcceptButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				OnAccept();
			}));
			if (m_AcceptLabel != null)
			{
				m_AcceptLabel.text = UIStrings.Instance.CommonTexts.Accept;
			}
		}
		m_CloseButton.Interactable = base.ViewModel.CloseEnabled.Value;
		if (base.ViewModel.CloseEnabled.Value)
		{
			AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				OnCancel();
			}));
			AddDisposable(EscHotkeyManager.Instance.Subscribe(base.OnCancel));
		}
	}
}
