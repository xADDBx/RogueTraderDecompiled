using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.BugReport.Console;

public class BugReportConsoleView : BugReportBaseView
{
	[Header("Hints")]
	[SerializeField]
	private CanvasGroup m_OpenBugReportGroup;

	[SerializeField]
	private TextMeshProUGUI m_OpenBugReportText;

	[SerializeField]
	private ConsoleHint m_FirstOpenBugReportHint;

	[SerializeField]
	private ConsoleHint m_SecondOpenBugReportHint;

	[SerializeField]
	private ConsoleHint m_SendHint;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_OpenBugReportText.text = UIStrings.Instance.UIBugReport.OpenBugReportText;
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		base.BuildNavigationImpl(navigationBehaviour);
		navigationBehaviour.FocusOnEntityManual(m_ContextDropdown);
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		base.CreateInputImpl(inputLayer);
		AddDisposable(m_FirstOpenBugReportHint.BindCustomAction(18, inputLayer));
		AddDisposable(m_SecondOpenBugReportHint.BindCustomAction(19, inputLayer));
		m_SendHint.SetLabel(UIStrings.Instance.UIBugReport.SendButton);
		AddDisposable(m_SendHint.Bind(m_InputLayer.AddButton(delegate
		{
			OnSend();
		}, 10, InputActionEventType.ButtonJustLongPressed)));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnClose();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnShowDrawing();
		}, 11), UIStrings.Instance.UIBugReport.EditScreenShotTitleText));
		AddDisposable(m_PrivacyToggle.IsOn.Subscribe(OnPrivacyToggle));
		AddDisposable(m_InputLayer.LayerBinded.Subscribe(OnLayerBinded));
	}

	private void OnLayerBinded(bool value)
	{
		m_OpenBugReportGroup.alpha = (value ? 1 : 0);
	}

	private void OnPrivacyToggle(bool isOn)
	{
		TrySetSendAlpha(isOn ? 1f : 0.2f);
	}

	private void TrySetSendAlpha(float value)
	{
		if (!(m_SendHint == null))
		{
			MonoBehaviour sendHint = m_SendHint;
			if ((object)sendHint != null && sendHint.TryGetComponent<CanvasGroup>(out var component))
			{
				component.alpha = value;
			}
		}
	}
}
