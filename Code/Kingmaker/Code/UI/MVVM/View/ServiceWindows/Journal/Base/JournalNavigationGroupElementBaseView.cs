using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.Pantograph;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class JournalNavigationGroupElementBaseView : ViewBase<JournalQuestVM>, IWidgetView, IQuestEntity, IConsoleNavigationEntity, IConsoleEntity, IUpdateCanCompleteOrderNotificationHandler, ISubscriber
{
	[Space]
	[SerializeField]
	[UsedImplicitly]
	protected OwlcatMultiButton m_MultiButton;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private bool m_SetLabelBold;

	[Header("Completion")]
	[SerializeField]
	[UsedImplicitly]
	private Image m_StatusImage;

	[SerializeField]
	private Sprite m_UpdatedPaperMark;

	[SerializeField]
	private Sprite m_NewQuestPaperMark;

	[SerializeField]
	private Sprite m_NewOrderPaperMark;

	[SerializeField]
	private Sprite m_NewRumourPaperMark;

	[SerializeField]
	private Sprite m_CompletedPaperMark;

	[SerializeField]
	private Sprite m_FailedPaperMark;

	[SerializeField]
	private Sprite m_PostponedPaperMark;

	[SerializeField]
	private Sprite m_AlarmPaperMark;

	[SerializeField]
	private Sprite m_Dlc1Mark;

	[SerializeField]
	private Sprite m_Dlc2Mark;

	[SerializeField]
	private Sprite m_UpdatedPantographMark;

	[SerializeField]
	private Sprite m_NewQuestPantographMark;

	[SerializeField]
	private Sprite m_NewOrderPantographMark;

	[SerializeField]
	private Sprite m_NewRumourPantographMark;

	[SerializeField]
	private Sprite m_CompletedPantographMark;

	[SerializeField]
	private Sprite m_FailedPantographMark;

	[SerializeField]
	private Sprite m_PostponedPantographMark;

	[SerializeField]
	private Sprite m_AlarmPantographMark;

	[SerializeField]
	private Sprite m_ReadyToCompletePantographMark;

	[FormerlySerializedAs("m_Dlc1PantographMark")]
	[SerializeField]
	private Sprite m_Dlc1NewQuestPantographMark;

	[FormerlySerializedAs("m_Dlc2PantographMark")]
	[SerializeField]
	private Sprite m_Dlc2NewQuestPantographMark;

	[SerializeField]
	private RectTransform m_ReadyToCompleteImage;

	[UsedImplicitly]
	private bool m_IsInit;

	private bool m_SelectNow;

	public bool IsActive => base.ViewModel?.IsActive ?? false;

	public MonoBehaviour MonoBehaviour => this;

	public PantographConfig PantographConfig { get; private set; }

	public bool IsSelected => base.ViewModel.IsSelected.Value;

	public Quest Quest => base.ViewModel?.Quest;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((JournalQuestVM)vm);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		m_Label.text = base.ViewModel.Title;
		AddDisposable(base.ViewModel.IsSelected.Subscribe(delegate(bool value)
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				OnSelected(value);
			}, 3);
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_MultiButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel?.SelectQuest();
		}));
		AddDisposable(base.ViewModel.IsOrderCompleted.Subscribe(delegate
		{
			if (base.ViewModel != null)
			{
				SetupStatusMark();
				SetupPantographConfig();
				OnSelected(base.ViewModel.IsSelected.Value);
			}
		}));
		SetupStatusMark();
		SetupPantographConfig();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetupStatusMark()
	{
		m_StatusImage.sprite = GetPaperStatusSprite();
		m_ReadyToCompleteImage.gameObject.SetActive(base.ViewModel.CanCompleteOrder && !base.ViewModel.IsOrderCompleted.Value && base.ViewModel.Quest.State != QuestState.Completed);
	}

	private Sprite GetPaperStatusSprite()
	{
		if (base.ViewModel == null)
		{
			return null;
		}
		Sprite result = base.ViewModel.Quest.Blueprint.Dlc switch
		{
			Dlc.Dlc1 => m_Dlc1Mark, 
			Dlc.Dlc2 => m_Dlc2Mark, 
			_ => UIConfig.Instance.TransparentImage, 
		};
		JournalQuestVM viewModel = base.ViewModel;
		if (viewModel != null)
		{
			if (viewModel.IsNew)
			{
				if (viewModel.IsRumour)
				{
					if (!viewModel.QuestIsViewed)
					{
						return m_NewRumourPaperMark;
					}
				}
				else if (viewModel.IsOrder)
				{
					if (!viewModel.QuestIsViewed)
					{
						return m_NewOrderPaperMark;
					}
				}
				else if (!viewModel.QuestIsViewed)
				{
					return m_NewQuestPaperMark;
				}
				if (viewModel.IsUpdated)
				{
					goto IL_00f1;
				}
				if (viewModel.IsCompleted)
				{
					goto IL_00fa;
				}
				if (viewModel.IsFailed)
				{
					goto IL_0103;
				}
				if (viewModel.IsPostponed)
				{
					goto IL_010c;
				}
				if (viewModel.IsViewed)
				{
					return result;
				}
			}
			else
			{
				if (viewModel.IsUpdated)
				{
					goto IL_00f1;
				}
				if (viewModel.IsCompleted)
				{
					goto IL_00fa;
				}
				if (viewModel.IsFailed)
				{
					goto IL_0103;
				}
				if (viewModel.IsPostponed)
				{
					goto IL_010c;
				}
			}
		}
		return null;
		IL_010c:
		return m_PostponedPaperMark;
		IL_0103:
		return m_FailedPaperMark;
		IL_00fa:
		return m_CompletedPaperMark;
		IL_00f1:
		return m_UpdatedPaperMark;
	}

	private Sprite GetPantographStatusSprite()
	{
		if (base.ViewModel == null)
		{
			return null;
		}
		Sprite result = base.ViewModel.Quest.Blueprint.Dlc switch
		{
			Dlc.Dlc1 => m_Dlc1NewQuestPantographMark, 
			Dlc.Dlc2 => m_Dlc2NewQuestPantographMark, 
			_ => UIConfig.Instance.TransparentImage, 
		};
		Sprite result2 = base.ViewModel.Quest.Blueprint.Dlc switch
		{
			Dlc.Dlc1 => m_Dlc1NewQuestPantographMark, 
			Dlc.Dlc2 => m_Dlc2NewQuestPantographMark, 
			_ => m_NewQuestPantographMark, 
		};
		JournalQuestVM viewModel = base.ViewModel;
		if (viewModel != null)
		{
			if (viewModel.IsNew)
			{
				if (viewModel.IsRumour)
				{
					if (!viewModel.QuestIsViewed)
					{
						return m_NewRumourPantographMark;
					}
				}
				else if (viewModel.IsOrder)
				{
					if (!viewModel.QuestIsViewed)
					{
						return m_NewOrderPantographMark;
					}
				}
				else if (!viewModel.QuestIsViewed)
				{
					return result2;
				}
				if (viewModel.IsUpdated)
				{
					goto IL_0138;
				}
				if (viewModel.IsCompleted)
				{
					goto IL_0141;
				}
				if (viewModel.IsFailed)
				{
					goto IL_014a;
				}
				if (viewModel.IsPostponed)
				{
					goto IL_0153;
				}
				if (viewModel.IsViewed)
				{
					return result;
				}
			}
			else
			{
				if (viewModel.IsUpdated)
				{
					goto IL_0138;
				}
				if (viewModel.IsCompleted)
				{
					goto IL_0141;
				}
				if (viewModel.IsFailed)
				{
					goto IL_014a;
				}
				if (viewModel.IsPostponed)
				{
					goto IL_0153;
				}
			}
		}
		return null;
		IL_014a:
		return m_FailedPantographMark;
		IL_0141:
		return m_CompletedPantographMark;
		IL_0153:
		return m_PostponedPantographMark;
		IL_0138:
		return m_UpdatedPantographMark;
	}

	private void OnSelected(bool value)
	{
		if (base.ViewModel == null)
		{
			return;
		}
		m_MultiButton.SetActiveLayer(value ? "On" : "Off");
		if (m_SetLabelBold)
		{
			m_Label.fontStyle = (value ? FontStyles.Bold : FontStyles.Normal);
		}
		if (value)
		{
			SetupPantographConfig();
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
			m_SelectNow = true;
		}
		if (!value && m_SelectNow)
		{
			foreach (QuestObjective objective in Quest.Objectives)
			{
				if (!objective.IsVisible || objective.State == QuestObjectiveState.None || objective.Blueprint.IsAddendum)
				{
					continue;
				}
				objective.NeedToAttention = false;
				IOrderedEnumerable<QuestObjective> orderedEnumerable = (from b in objective?.Blueprint?.Addendums?.Where((BlueprintQuestObjective b) => b != null)
					select objective?.Quest?.TryGetObjective(b) into a
					where a != null
					where a.IsVisible
					orderby a?.Order descending
					select a);
				if (orderedEnumerable == null)
				{
					continue;
				}
				foreach (QuestObjective item in orderedEnumerable)
				{
					item.NeedToAttention = false;
				}
			}
			m_SelectNow = false;
		}
		SetupStatusMark();
	}

	private void SetupPantographConfig()
	{
		if (base.ViewModel == null)
		{
			return;
		}
		bool flag = base.ViewModel.CanCompleteOrder && !base.ViewModel.IsOrderCompleted.Value;
		if (base.ViewModel.IsNew && base.ViewModel.IsViewed)
		{
			PantographConfig = new PantographConfig(base.transform, m_Label.text, flag ? new List<Sprite> { m_ReadyToCompletePantographMark } : new List<Sprite> { GetPantographStatusSprite() });
			return;
		}
		List<Sprite> list = new List<Sprite> { GetPantographStatusSprite() };
		if (flag)
		{
			list.Add(m_ReadyToCompletePantographMark);
		}
		PantographConfig = new PantographConfig(base.transform, m_Label.text, list);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is JournalQuestVM;
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		return m_MultiButton.IsValid();
	}

	public void HandleUpdateCanCompleteOrderNotificationInJournal()
	{
		if (base.ViewModel != null)
		{
			m_ReadyToCompleteImage.gameObject.SetActive(base.ViewModel.CanCompleteOrder && !base.ViewModel.IsOrderCompleted.Value && base.ViewModel.Quest.State != QuestState.Completed);
		}
	}
}
