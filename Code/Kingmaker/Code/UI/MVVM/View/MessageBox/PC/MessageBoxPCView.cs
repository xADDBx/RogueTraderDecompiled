using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.MessageBox.PC;

public class MessageBoxPCView : MessageBoxBaseView
{
	[Header("Input Field")]
	[SerializeField]
	protected TMP_InputField m_InputField;

	[Header("Buttons Block")]
	[SerializeField]
	protected OwlcatButton m_AcceptButton;

	[SerializeField]
	protected OwlcatButton m_DeclineButton;

	[SerializeField]
	private TextMeshProUGUI m_AcceptText;

	[SerializeField]
	private TextMeshProUGUI m_DeclineText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_AcceptButton.gameObject.SetActive(!base.ViewModel.IsProgressBar.Value);
		m_DeclineButton.gameObject.SetActive(base.ViewModel.ShowDecline.Value);
		AddDisposable(ObservableExtensions.Subscribe(m_DeclineButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnDeclinePressed();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_AcceptButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnAcceptPressed();
		}));
		AddDisposable(m_DontShowToggle.OnPointerClickAsObservable().Subscribe(delegate
		{
			UISounds.Instance.Sounds.Tutorial.BanTutorialType.Play();
		}));
	}

	protected override void SetAcceptInteractable(bool interactable)
	{
		m_AcceptButton.Interactable = interactable;
	}

	protected override void SetAcceptText(string acceptText)
	{
		m_AcceptText.text = acceptText;
	}

	protected override void SetDeclineText(string declineText)
	{
		m_DeclineText.text = declineText;
	}

	protected override void BindTextField()
	{
		m_InputField.gameObject.SetActive(base.ViewModel.BoxType == DialogMessageBoxBase.BoxType.TextField);
		if (base.ViewModel.BoxType == DialogMessageBoxBase.BoxType.TextField)
		{
			if (m_InputField.placeholder is TextMeshProUGUI textMeshProUGUI)
			{
				textMeshProUGUI.text = base.ViewModel.InputPlaceholder;
			}
			AddDisposable(base.ViewModel.InputText.Subscribe(delegate(string value)
			{
				m_InputField.text = value;
			}));
			m_InputField.onValueChanged.AddListener(OnTextInputChanged);
		}
	}

	protected override void DestroyTextField()
	{
		m_InputField.onValueChanged.RemoveListener(OnTextInputChanged);
	}

	protected override void BindProgressBar()
	{
	}
}
