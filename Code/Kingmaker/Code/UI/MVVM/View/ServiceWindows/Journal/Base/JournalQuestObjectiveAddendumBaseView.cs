using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class JournalQuestObjectiveAddendumBaseView : ViewBase<JournalQuestObjectiveAddendumVM>, IWidgetView
{
	[Header("Description")]
	[SerializeField]
	[UsedImplicitly]
	protected TextMeshProUGUI m_Description;

	[SerializeField]
	[UsedImplicitly]
	protected TextMeshProUGUI m_EtudeCounter;

	[SerializeField]
	protected TextMeshProUGUI m_Destination;

	[Header("Completion")]
	[SerializeField]
	[UsedImplicitly]
	private GameObject m_FailedMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_CompletedMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_AttentionMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_InProgressMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_UpdatedMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_PostponedMark;

	[SerializeField]
	private Color m_InProgressColor;

	[SerializeField]
	private Color m_CompletedColor;

	[SerializeField]
	private Color m_FailedColor;

	[SerializeField]
	[UsedImplicitly]
	private Image m_DestinationGeoMark;

	[UsedImplicitly]
	private bool m_IsInit;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((JournalQuestObjectiveAddendumVM)vm);
	}

	protected override void BindViewImplementation()
	{
		SetupView();
	}

	private void SetupView()
	{
		SetupDescription();
		SetupState();
	}

	protected virtual void SetupDescription()
	{
		m_Description.text = "- " + base.ViewModel.Description;
		m_Description.color = (base.ViewModel.IsFailed ? m_FailedColor : (base.ViewModel.IsCompleted ? m_CompletedColor : m_InProgressColor));
		if (m_Destination != null)
		{
			bool flag = !string.IsNullOrWhiteSpace(base.ViewModel.Destination);
			m_Destination.transform.parent.gameObject.SetActive(flag);
			m_Destination.text = (flag ? base.ViewModel.Destination : string.Empty);
		}
		bool hasEtudeCounter = base.ViewModel.HasEtudeCounter;
		m_EtudeCounter.gameObject.SetActive(hasEtudeCounter);
		if (hasEtudeCounter)
		{
			m_EtudeCounter.text = $"{base.ViewModel.CurrentEtudeCounter}/{base.ViewModel.MinEtudeCounter} {base.ViewModel.EtudeCounterDescription}";
		}
	}

	protected virtual void SetupState()
	{
		m_FailedMark.gameObject.SetActive(base.ViewModel.IsFailed);
		m_CompletedMark.gameObject.SetActive(base.ViewModel.IsCompleted);
		m_AttentionMark.gameObject.SetActive(!base.ViewModel.IsViewed && !base.ViewModel.IsFailed && !base.ViewModel.IsCompleted);
		m_InProgressMark.gameObject.SetActive(base.ViewModel.IsViewed && !base.ViewModel.IsFailed && !base.ViewModel.IsCompleted);
	}

	protected string GetHintText()
	{
		UIQuestNotificationTexts questNotificationTexts = UIStrings.Instance.QuestNotificationTexts;
		JournalQuestObjectiveAddendumVM viewModel = base.ViewModel;
		if (viewModel != null)
		{
			if (!viewModel.IsFailed)
			{
				if (!viewModel.IsCompleted)
				{
					if (!viewModel.IsViewed)
					{
						return questNotificationTexts.QuestNew;
					}
					return questNotificationTexts.QuestStarted;
				}
				return questNotificationTexts.QuestComplite;
			}
			return questNotificationTexts.QuestFailed;
		}
		return string.Empty;
	}

	protected override void DestroyViewImplementation()
	{
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is JournalQuestObjectiveAddendumVM;
	}
}
