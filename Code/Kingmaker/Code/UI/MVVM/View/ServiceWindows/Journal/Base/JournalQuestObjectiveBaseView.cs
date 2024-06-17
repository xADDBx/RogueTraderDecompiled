using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class JournalQuestObjectiveBaseView : ViewBase<JournalQuestObjectiveVM>, IWidgetView
{
	[Space]
	[Header("Texts")]
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_ObjectiveNummer;

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
	private GameObject m_AttentionMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_FailedMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_CompletedMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_UpdatedMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_PostponedMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_InProgressMark;

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
		Bind((JournalQuestObjectiveVM)vm);
	}

	protected override void BindViewImplementation()
	{
		SetupState();
	}

	public virtual void SetupHeader()
	{
		m_Title.text = base.ViewModel.Title;
		m_ObjectiveNummer.text = $"-//*{base.ViewModel.ObjectiveNumber:D3}--";
		SetupBody();
	}

	private void SetupBody()
	{
		bool flag = !string.IsNullOrWhiteSpace(base.ViewModel.Description);
		bool flag2 = !string.IsNullOrWhiteSpace(base.ViewModel.Destination);
		m_Destination.transform.parent.gameObject.SetActive(flag2);
		m_Description.text = (flag ? base.ViewModel.Description : string.Empty);
		bool hasEtudeCounter = base.ViewModel.HasEtudeCounter;
		m_EtudeCounter.gameObject.SetActive(hasEtudeCounter);
		if (hasEtudeCounter)
		{
			m_EtudeCounter.text = $"{base.ViewModel.CurrentEtudeCounter}/{base.ViewModel.MinEtudeCounter} {base.ViewModel.EtudeCounterDescription}";
		}
		m_Destination.text = (flag2 ? base.ViewModel.Destination : string.Empty);
		if (flag2)
		{
			AddDisposable(m_Destination.SetTooltip(new TooltipTemplateGlobalMapPosition(), new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)));
		}
	}

	protected virtual void SetupState()
	{
		m_FailedMark.SetActive(base.ViewModel.IsFailed);
		m_CompletedMark.SetActive(base.ViewModel.IsCompleted);
		m_AttentionMark.SetActive(!base.ViewModel.IsViewed && !base.ViewModel.IsFailed && !base.ViewModel.IsCompleted && !base.ViewModel.IsPostponed);
		m_PostponedMark.SetActive(base.ViewModel.IsPostponed);
		m_InProgressMark.SetActive(!base.ViewModel.IsFailed && !base.ViewModel.IsCompleted && base.ViewModel.IsViewed && !base.ViewModel.IsPostponed);
	}

	protected string GetHintText()
	{
		UIQuestNotificationTexts questNotificationTexts = UIStrings.Instance.QuestNotificationTexts;
		JournalQuestObjectiveVM viewModel = base.ViewModel;
		if (viewModel != null)
		{
			if (!viewModel.IsFailed)
			{
				if (!viewModel.IsCompleted)
				{
					if (!viewModel.IsViewed)
					{
						if (!viewModel.IsPostponed)
						{
							return questNotificationTexts.QuestNew;
						}
					}
					else if (!viewModel.IsPostponed)
					{
						return questNotificationTexts.QuestStarted;
					}
					return questNotificationTexts.QuestPostponed;
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
		return viewModel is JournalQuestObjectiveVM;
	}
}
