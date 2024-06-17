using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.QuestNotification;

public class QuestNotificationQuestView : ViewBase<QuestNotificationQuestVM>
{
	private bool m_IsInit;

	[SerializeField]
	private TextMeshProUGUI m_StatusLabel;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private GameObject m_FailMark;

	[SerializeField]
	private GameObject m_CompleteMark;

	[SerializeField]
	private GameObject m_NewMark;

	[SerializeField]
	private GameObject m_UpdatedMark;

	[SerializeField]
	private GameObject m_PostponedMark;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			Clear();
			Hide();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		if (m_Description != null && base.ViewModel.State == QuestNotificationState.New)
		{
			m_Description.gameObject.SetActive(value: true);
			m_Description.text = base.ViewModel.Description;
		}
		m_FailMark.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Failed);
		m_CompleteMark.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Completed);
		m_NewMark.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.New);
		m_UpdatedMark.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Updated);
		m_PostponedMark.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Postponed);
		m_StatusLabel.color = UIConfig.Instance.QuestStateColor.GetQuestStateColor(base.ViewModel.State);
		m_StatusLabel.text = UIStrings.Instance.QuestNotificationTexts.GetQuestNotificationStateText(base.ViewModel.State, base.ViewModel.Quest.Blueprint.Type, base.ViewModel.Quest.Blueprint.Group);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Clear()
	{
		m_Description.text = string.Empty;
		m_Description.gameObject.SetActive(value: false);
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
	}
}
