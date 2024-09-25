using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Colonization.PC;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;

public class JournalOrderPCView : BaseJournalItemPCView
{
	[Header("Header")]
	[SerializeField]
	private ScrambledTMP m_TitleLabel;

	[Header("Location Info")]
	[SerializeField]
	private TextMeshProUGUI m_PlaceLabel;

	[Header("Completion")]
	[SerializeField]
	private GameObject m_CompletionItem;

	[SerializeField]
	private TextMeshProUGUI m_CompletionLabel;

	[Header("Content")]
	[SerializeField]
	private TextMeshProUGUI m_ServiceMessageLabel;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionLabel;

	[Header("Objectives")]
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[Header("Nomos")]
	[SerializeField]
	private GameObject m_NomosTag;

	[Header("Orders")]
	[SerializeField]
	private GameObject m_CompletionGroup;

	[SerializeField]
	private OwlcatButton m_CompleteOrderButton;

	[SerializeField]
	private TextMeshProUGUI m_CompleteOrderLabel;

	[Header("Requirements Group Objects")]
	[SerializeField]
	private TextMeshProUGUI m_RequirementsTitle;

	[SerializeField]
	protected WidgetListMVVM m_RequirementsWidgetList;

	[SerializeField]
	private ColonyProjectsRequirementElementPCView m_RequirementsViewPrefab;

	[Header("Rewards Group Objects")]
	[SerializeField]
	private TextMeshProUGUI m_RewardsTitle;

	[SerializeField]
	protected WidgetListMVVM m_RewardsWidgetList;

	[SerializeField]
	private ColonyProjectsRewardElementPCView m_RewardsViewPrefab;

	[Header("Resources")]
	[SerializeField]
	private WidgetListMVVM m_WidgetListResources;

	[SerializeField]
	private TextMeshProUGUI m_YourResourcesText;

	[SerializeField]
	private JournalOrderResourcesPCView m_JournalOrderResourcesPCViewPrefab;

	[SerializeField]
	private JournalOrderProfitFactorPCView m_JournalOrderProfitFactorPCViewPrefab;

	[SerializeField]
	private float m_DefaultFontSize = 21f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsOrderCompleted.Subscribe(delegate
		{
			UpdateView();
		}));
		AddDisposable(m_CompleteOrderButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CompleteOrder();
		}));
		AddDisposable(base.ViewModel.RefreshData.Subscribe(delegate
		{
			SetupBasement();
		}));
		m_RequirementsTitle.text = UIStrings.Instance.QuesJournalTexts.RequiredResources;
		m_RewardsTitle.text = UIStrings.Instance.QuesJournalTexts.RewardsResources;
		m_CompleteOrderLabel.text = UIStrings.Instance.QuesJournalTexts.CompleteOrder;
		m_YourResourcesText.text = string.Concat("- [ ", UIStrings.Instance.QuesJournalTexts.OrderResourcesYourResources, " ] -");
		m_DescriptionLabel.fontSize = m_DefaultFontSize * base.ViewModel.FontMultiplier;
	}

	private void DrawRewards()
	{
		m_RewardsWidgetList.DrawEntries(base.ViewModel.Rewards, m_RewardsViewPrefab);
	}

	private void DrawRequirements()
	{
		m_RequirementsWidgetList.DrawEntries(base.ViewModel.Requirements, m_RequirementsViewPrefab);
	}

	public void CompleteOrder()
	{
		base.ViewModel.CompleteOrder();
		UpdateView();
	}

	protected override void UpdateView()
	{
		SetupHeader();
		SetupBody();
		SetupBasement();
		ScrollToTop();
		base.UpdateView();
	}

	private void SetupHeader()
	{
		m_TitleLabel.SetText(string.Empty, base.ViewModel.Title);
		m_PlaceLabel.text = base.ViewModel.Place;
	}

	private void SetupBody()
	{
		SetTextItem(m_ServiceMessageLabel.gameObject, m_ServiceMessageLabel, base.ViewModel.ServiceMessage);
		SetTextItem(m_DescriptionLabel.gameObject, m_DescriptionLabel, base.ViewModel.Description);
		SetTextItem(m_CompletionItem, m_CompletionLabel, base.ViewModel.CompletionText);
		m_NomosTag.SetActive(base.ViewModel.IsAffectedByNomos);
		UpdateOrderStatus();
		UpdateOrderRequirementsAndRewards();
	}

	private void SetupBasement()
	{
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetListResources.DrawEntries(base.ViewModel.ResourcesVMs, m_JournalOrderResourcesPCViewPrefab);
		m_JournalOrderProfitFactorPCViewPrefab.Bind(base.ViewModel.JournalOrderProfitFactorVM);
	}

	private void UpdateOrderRequirementsAndRewards()
	{
		m_CompletionGroup.SetActive(!base.ViewModel.IsOrderCompleted.Value && base.ViewModel.Quest.State != QuestState.Completed);
		m_CompleteOrderButton.Interactable = base.ViewModel.CanCompleteOrder && !base.ViewModel.IsOrderCompleted.Value;
		m_CompleteOrderButton.gameObject.SetActive(!base.ViewModel.IsOrderCompleted.Value);
		DrawRewards();
		DrawRequirements();
	}

	private void UpdateOrderStatus()
	{
		SetupStatuses();
	}

	private string GetRewardHintText(RewardUI reward)
	{
		string text;
		if (reward.Colony == null)
		{
			text = reward.Description;
			if (text == null)
			{
				return "";
			}
		}
		else
		{
			text = reward.Description + " (" + reward.Colony.Name.Text + ")";
		}
		return text;
	}

	public void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}
}
