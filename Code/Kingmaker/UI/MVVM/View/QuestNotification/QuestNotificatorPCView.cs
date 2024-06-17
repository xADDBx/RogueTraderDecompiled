using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.QuestNotification;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.QuestNotification;

public class QuestNotificatorPCView : QuestNotificatorBaseView
{
	[SerializeField]
	private OwlcatButton m_JournalButton;

	[SerializeField]
	private OwlcatButton m_FullBodyButton;

	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private TextMeshProUGUI m_ToJournalButtonLabel;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CloseButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			Close();
		}));
		AddDisposable(m_JournalButton.OnLeftClickAsObservable().Subscribe(base.OpenJournal));
		if (m_FullBodyButton != null)
		{
			AddDisposable(m_FullBodyButton.OnLeftClickAsObservable().Subscribe(base.OpenJournal));
		}
		m_ToJournalButtonLabel.text = UIStrings.Instance.QuestNotificationTexts.ToJournal;
	}

	protected override void CheckJournalButtons()
	{
		base.CheckJournalButtons();
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_JournalButton.transform.parent.gameObject.SetActive(CheckActiveToJournalButtons());
			m_FullBodyButton.SetInteractable(CheckActiveToJournalButtons());
		}, 1);
	}
}
