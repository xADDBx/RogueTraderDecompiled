using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.QuestNotification;

public class QuestNotificationObjectivesView : ViewBase<QuestNotificationEntityVM>
{
	private bool m_IsInit;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private GameObject m_FailMark;

	[SerializeField]
	private GameObject m_UpdateMark;

	[SerializeField]
	private GameObject m_PostponeMark;

	[SerializeField]
	private GameObject m_CompleteMark;

	[SerializeField]
	private GameObject m_NewMark;

	[SerializeField]
	private QuestNotificationObjectivesView m_AdditionalObjective;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			Hide();
			m_IsInit = true;
			if (m_AdditionalObjective != null)
			{
				m_AdditionalObjective.Initialize();
			}
		}
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_Title.gameObject.SetActive(!string.IsNullOrWhiteSpace(base.ViewModel.Title));
		m_Title.text = base.ViewModel.Title;
		m_Description.gameObject.SetActive(value: false);
		SetMark();
		AddDisposable(base.ViewModel.AdditionalObjective.Subscribe(delegate
		{
			BindAdditionalObjective();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void SetMark()
	{
		m_FailMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Failed);
		m_CompleteMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Completed);
		m_NewMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.New);
		m_UpdateMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Updated);
		m_PostponeMark.Or(null)?.gameObject.SetActive(base.ViewModel.State == QuestNotificationState.Postponed);
	}

	private void BindAdditionalObjective()
	{
		if (m_AdditionalObjective != null && !string.IsNullOrWhiteSpace(base.ViewModel?.AdditionalObjective?.Value?.Title))
		{
			m_AdditionalObjective.Bind(base.ViewModel.AdditionalObjective.Value);
		}
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
