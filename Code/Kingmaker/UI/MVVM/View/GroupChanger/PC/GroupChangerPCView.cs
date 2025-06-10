using Kingmaker.Blueprints.Root.Strings;
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
		if (m_AcceptLabel != null)
		{
			m_AcceptLabel.text = UIStrings.Instance.CommonTexts.Accept;
		}
		AddDisposable(m_AcceptButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnAccept();
		}));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnCancel();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		EscHotkeyManager.Instance.Unsubscribe(base.OnCancel);
	}

	protected override void CheckCoopButtonsImpl(bool isMainCharacter)
	{
		base.CheckCoopButtonsImpl(isMainCharacter);
		m_AcceptButton.Or(null)?.gameObject.SetActive(isMainCharacter);
		m_CloseButton.Interactable = base.ViewModel.CloseEnabled.Value && isMainCharacter;
		if (isMainCharacter && base.ViewModel.CloseEnabled.Value)
		{
			EscHotkeyManager.Instance.Subscribe(base.OnCancel);
		}
		else
		{
			EscHotkeyManager.Instance.Unsubscribe(base.OnCancel);
		}
	}
}
