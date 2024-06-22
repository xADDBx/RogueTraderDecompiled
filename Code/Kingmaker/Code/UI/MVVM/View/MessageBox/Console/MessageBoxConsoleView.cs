using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.MessageBox.Console;

public class MessageBoxConsoleView : MessageBoxBaseView
{
	[Header("Input Field")]
	[SerializeField]
	protected ConsoleInputField m_InputField;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	public static readonly string InputLayerName = "MessageBoxInputContext";

	protected readonly BoolReactiveProperty ConfirmBindActive = new BoolReactiveProperty();

	private ConsoleHintDescription m_ConfirmHint;

	private ConsoleHintDescription m_DeclineHint;

	protected override void BindViewImplementation()
	{
		CreateInput();
		base.BindViewImplementation();
		AddDisposable(GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged));
	}

	protected override void SetAcceptInteractable(bool interactable)
	{
		ConfirmBindActive.Value = interactable;
	}

	protected override void SetAcceptText(string acceptText)
	{
		m_ConfirmHint.SetLabel(acceptText);
	}

	protected override void SetDeclineText(string declineText)
	{
		m_DeclineHint.SetLabel(declineText);
	}

	protected override void BindTextField()
	{
		m_InputField.gameObject.SetActive(base.ViewModel.BoxType == DialogMessageBoxBase.BoxType.TextField);
		if (base.ViewModel.BoxType == DialogMessageBoxBase.BoxType.TextField)
		{
			m_InputField.SetPlaceholderText(base.ViewModel.InputPlaceholder);
			AddDisposable(base.ViewModel.InputText.Subscribe(delegate(string value)
			{
				m_InputField.Text = value;
			}));
			m_InputField.InputField.onValueChanged.AddListener(OnTextInputChanged);
			m_InputField.Select();
		}
	}

	protected override void DestroyTextField()
	{
		m_InputField.InputField.onValueChanged.RemoveListener(OnTextInputChanged);
		m_InputField.Abort();
	}

	protected override void BindProgressBar()
	{
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = InputLayerName
		};
		CreateInputImpl(m_InputLayer, m_HintsWidget);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(m_DeclineHint = hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9, base.ViewModel.ShowDecline)) as ConsoleHintDescription);
		AddDisposable(m_ConfirmHint = hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, ConfirmBindActive.And(base.ViewModel.IsProgressBar.Not()).ToReactiveProperty())) as ConsoleHintDescription);
		AddDisposable(inputLayer.AddButton(delegate
		{
			if (!base.ViewModel.ShowDecline.Value && !base.ViewModel.IsProgressBar.Value)
			{
				OnDeclineClick();
			}
		}, 9));
		AddDisposable(inputLayer.AddAxis(Scroll, 3, repeat: true));
	}

	protected virtual void OnConfirmClick()
	{
		base.ViewModel.OnAcceptPressed();
	}

	protected virtual void OnDeclineClick()
	{
		base.ViewModel.OnDeclinePressed();
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	private void OnCurrentInputLayerChanged()
	{
		if (GamePad.Instance.CurrentInputLayer != m_InputLayer && !(GamePad.Instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName))
		{
			GamePad.Instance.PopLayer(m_InputLayer);
			GamePad.Instance.PushLayer(m_InputLayer);
		}
	}
}
