using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.GameCommands;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class BaseJournalItemConsoleView : BaseJournalItemBaseView
{
	[Header("Navigation Group Objects")]
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private JournalQuestObjectiveConsoleView m_QuestObjectiveViewPrefab;

	[Header("Statuses")]
	[SerializeField]
	private TextMeshProUGUI m_StatusLabel;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_NewMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_DefaultMark;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_AlarmMark;

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
	private Color m_NewColor;

	[SerializeField]
	private Color m_DefaultColor;

	[SerializeField]
	private Color m_AlarmColor;

	[SerializeField]
	private Color m_FailedColor;

	[SerializeField]
	private Color m_CompletedColor;

	[SerializeField]
	private Color m_UpdatedColor;

	[SerializeField]
	private Color m_PostponedColor;

	protected override void UpdateView()
	{
		if (m_QuestObjectiveViewPrefab != null)
		{
			DrawEntities();
		}
		DelayedInvoker.InvokeInFrames(CheckQuestIsViewed, 1);
		base.UpdateView();
	}

	private void CheckQuestIsViewed()
	{
		if (!base.ViewModel.Quest.IsViewed)
		{
			Game.Instance.GameCommandQueue.SetQuestViewed(base.ViewModel.Quest);
		}
		base.ViewModel.Quest.Objectives.Where((QuestObjective o) => o.IsActive).ToList().ForEach(delegate(QuestObjective x)
		{
			if (!x.IsViewed)
			{
				Game.Instance.GameCommandQueue.SetQuestObjectiveViewed(x);
			}
		});
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Objectives.ToArray(), m_QuestObjectiveViewPrefab);
	}

	protected void SetupStatuses()
	{
		m_NewMark.SetActive(base.ViewModel.IsNew && !base.ViewModel.QuestIsViewed);
		m_DefaultMark.SetActive(base.ViewModel.IsNew && base.ViewModel.QuestIsViewed && !base.ViewModel.IsUpdated);
		m_FailedMark.SetActive(base.ViewModel.IsFailed);
		m_CompletedMark.SetActive(base.ViewModel.IsCompleted);
		m_UpdatedMark.SetActive(base.ViewModel.IsUpdated);
		m_PostponedMark.SetActive(base.ViewModel.IsPostponed);
		int num = ((!base.ViewModel.IsNew || base.ViewModel.QuestIsViewed) ? (base.ViewModel.IsUpdated ? 1 : (base.ViewModel.IsCompleted ? 2 : (-1))) : 0);
		m_StatusLabel.color = GetStatusColor();
		if (num == -1)
		{
			m_StatusLabel.text = UIStrings.Instance.QuestNotificationTexts.GetQuestStateText(base.ViewModel.Quest.State);
		}
		else
		{
			m_StatusLabel.text = UIStrings.Instance.QuestNotificationTexts.GetQuestStateText(num, base.ViewModel.Quest.Blueprint.Type, base.ViewModel.Quest.Blueprint.Group);
		}
	}

	private Color GetStatusColor()
	{
		JournalQuestVM viewModel = base.ViewModel;
		if (viewModel != null)
		{
			if (viewModel.IsNew)
			{
				if (!viewModel.QuestIsViewed)
				{
					return m_NewColor;
				}
				if (viewModel.IsViewed)
				{
					return m_DefaultColor;
				}
			}
			if (viewModel.IsFailed)
			{
				return m_FailedColor;
			}
			if (viewModel.IsCompleted)
			{
				return m_CompletedColor;
			}
			if (viewModel.IsUpdated)
			{
				return m_UpdatedColor;
			}
			if (viewModel.IsPostponed)
			{
				return m_PostponedColor;
			}
		}
		return Color.white;
	}
}
