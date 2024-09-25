using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.WarningNotification;

public class WarningsTextView : ViewBase<WarningsTextVM>
{
	[SerializeField]
	[UsedImplicitly]
	private WarningTextElement m_CommonWarning;

	[SerializeField]
	[UsedImplicitly]
	private WarningTextWithCountElement m_CounterElement;

	[SerializeField]
	[UsedImplicitly]
	private WarningTextElement m_AttentionElement;

	[SerializeField]
	[UsedImplicitly]
	private WarningTextElement m_WarningElement;

	[SerializeField]
	[UsedImplicitly]
	private WarningTextElement m_BigNotificationElement;

	[Header("Timings")]
	[SerializeField]
	private float m_FadeShowHide = 0.2f;

	[SerializeField]
	private float m_FadeStayOnTheScreen = 2f;

	public void Initialize()
	{
		m_CommonWarning.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShowString.Skip(1).Subscribe(delegate
		{
			Show(base.ViewModel.ShowString.Value, base.ViewModel.ShowFormat.Value);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void Show(string text, WarningNotificationFormat format)
	{
		WarningTextElement warningTextElement;
		switch (format)
		{
		case WarningNotificationFormat.Common:
			m_CommonWarning.SetText(text);
			warningTextElement = m_CommonWarning;
			break;
		case WarningNotificationFormat.Counter:
			m_CounterElement.SetText(UIStrings.Instance.SpaceCombatTexts.TimeSurvivalActionHint, text);
			warningTextElement = m_CounterElement;
			break;
		case WarningNotificationFormat.Attention:
			m_AttentionElement.SetText(text);
			warningTextElement = m_AttentionElement;
			break;
		case WarningNotificationFormat.Short:
			m_WarningElement.SetText(text);
			warningTextElement = m_WarningElement;
			break;
		case WarningNotificationFormat.BigNotification:
			m_BigNotificationElement.SetText(text);
			warningTextElement = m_BigNotificationElement;
			break;
		default:
			m_CommonWarning.SetText(text);
			warningTextElement = m_CommonWarning;
			break;
		}
		warningTextElement.ShowSequenceCanvasGroupFadeAnimation(m_FadeShowHide, m_FadeStayOnTheScreen, base.ViewModel.ShowWithSound);
	}
}
