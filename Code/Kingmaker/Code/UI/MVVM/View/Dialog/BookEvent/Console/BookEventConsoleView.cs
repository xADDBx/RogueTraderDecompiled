using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent.Console;

public class BookEventConsoleView : BookEventBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	[Header("Navigations")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	private FloatConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	private InputLayer m_GlossaryInputLayer;

	private readonly BoolReactiveProperty m_HasGlossaryPoints = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_GlossaryMode = new BoolReactiveProperty();

	private IDisposable m_GlossaryDisposable;

	private const string GlossaryInputLayerContextName = "DialogGlossary";

	private InputLayer m_VotesInputLayer;

	private GridConsoleNavigationBehaviour m_VotesNavigationBehavior;

	private readonly BoolReactiveProperty m_VotesMode = new BoolReactiveProperty();

	private const string VotesInputLayerContextName = "DialogVotes";

	public override void Initialize()
	{
		base.Initialize();
		m_FirstGlossaryFocus.Or(null)?.gameObject.SetActive(value: false);
		m_SecondGlossaryFocus.Or(null)?.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ConsoleHintsWidget.BindHint(Layer.AddButton(SwitchHistory, 10, base.IsShowHistory), UIStrings.Instance.BookEvent.BookEventCloseHistory));
		AddDisposable(m_ConsoleHintsWidget.BindHint(Layer.AddButton(SwitchHistory, 10, base.IsShowHistory.Not().ToReactiveProperty()), UIStrings.Instance.BookEvent.BookEventOpenHistory));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		CloseGlossary(default(InputActionEventData));
		m_GlossaryDisposable?.Dispose();
		m_GlossaryDisposable = null;
	}

	protected override void CreateInputImpl(InputLayer inputLayer, GridConsoleNavigationBehaviour behaviour)
	{
		base.CreateInputImpl(inputLayer, behaviour);
		AddDisposable(m_GlossaryNavigationBehavior = new FloatConsoleNavigationBehaviour(m_NavigationParameters));
		m_GlossaryInputLayer = m_GlossaryNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "DialogGlossary"
		});
		AddDisposable(m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Subscribe(OnGlossaryFocusChanged));
		AddDisposable(behaviour.Focus.Subscribe(ScrollMenu));
		AddDisposable(m_ConsoleHintsWidget.BindHint(Layer.AddButton(ShowGlossary, 11, m_HasGlossaryPoints.And(m_VotesMode.Not()).ToReactiveProperty()), UIStrings.Instance.Dialog.OpenGlossary));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_GlossaryInputLayer.AddButton(CloseGlossary, 9, m_GlossaryMode.And(m_VotesMode.Not()).ToReactiveProperty()), UIStrings.Instance.Dialog.CloseGlossary));
		AddDisposable(m_ConsoleHintsWidget.BindHint(Layer.AddButton(delegate
		{
			ShowVotes();
		}, 19, m_VotesMode.Not().And(VotesIsActive).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.Dialog.ShowVotes));
		AddDisposable(Layer.AddButton(OnShowEscMenu, 16, InputActionEventType.ButtonJustReleased));
		AddVotesInput();
	}

	private void AddVotesInput()
	{
		AddDisposable(m_VotesNavigationBehavior = new GridConsoleNavigationBehaviour());
		m_VotesInputLayer = m_VotesNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "DialogVotes"
		});
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_VotesInputLayer.AddButton(delegate
		{
			CloseVotes();
		}, 19, m_VotesMode, InputActionEventType.ButtonJustReleased), UIStrings.Instance.Dialog.HideVotes));
		AddDisposable(m_VotesInputLayer.AddButton(delegate
		{
			CloseVotes();
		}, 9, m_VotesMode));
	}

	private void ShowVotes()
	{
		m_VotesMode.Value = true;
		SetVotesNavigation();
		GamePad.Instance.PushLayer(m_VotesInputLayer);
		m_VotesNavigationBehavior.FocusOnFirstValidEntity();
	}

	private void CloseVotes()
	{
		m_VotesNavigationBehavior.UnFocusCurrentEntity();
		m_VotesMode.Value = false;
		TooltipHelper.HideTooltip();
		GamePad.Instance.PopLayer(m_VotesInputLayer);
	}

	private void SetVotesNavigation()
	{
		m_VotesNavigationBehavior.Clear();
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		list.AddRange(AnswersEntities.Select((BookEventAnswerView a) => a.DialogVotesBlock.FocusButton));
		List<OwlcatSelectable> list2 = (from block in list.OfType<OwlcatSelectable>()
			where block.IsValid()
			select block).ToList();
		if (list2.Any())
		{
			m_VotesNavigationBehavior.SetEntitiesVertical(list2);
		}
		if (m_VotesMode.Value)
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void ShowGlossary(InputActionEventData data)
	{
		IFloatConsoleNavigationEntity floatConsoleNavigationEntity = CalculateGlossary();
		m_GlossaryMode.Value = true;
		m_GlossaryDisposable = GamePad.Instance.PushLayer(m_GlossaryInputLayer);
		if (floatConsoleNavigationEntity != null)
		{
			m_GlossaryNavigationBehavior.FocusOnEntityManual(floatConsoleNavigationEntity);
		}
		else
		{
			m_GlossaryNavigationBehavior.FocusOnFirstValidEntity();
		}
	}

	private void CloseGlossary(InputActionEventData data)
	{
		m_GlossaryNavigationBehavior.UnFocusCurrentEntity();
		m_GlossaryDisposable?.Dispose();
		m_GlossaryDisposable = null;
		TooltipHelper.HideTooltip();
		m_GlossaryMode.Value = false;
	}

	protected override void UpdateFocusLinks()
	{
		base.UpdateFocusLinks();
		CalculateGlossary();
	}

	private IFloatConsoleNavigationEntity CalculateGlossary()
	{
		m_GlossaryNavigationBehavior.Clear();
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		if (!base.IsShowHistory.Value)
		{
			foreach (BookEventCueView currentCuesView in CurrentCuesViews)
			{
				list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(currentCuesView.Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusCueLink, currentCuesView.SkillChecks, TooltipHelper.GetLinkTooltipTemplate));
			}
		}
		if (base.IsShowHistory.Value)
		{
			for (int num = MemorizedCuesViews.Count - 1; num >= 0; num--)
			{
				list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(MemorizedCuesViews[num].Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusHistoryLink, TooltipHelper.GetLinkTooltipTemplate));
			}
		}
		if (!AnswersEntities.Empty() && !base.IsShowHistory.Value)
		{
			foreach (BookEventAnswerView answersEntity in AnswersEntities)
			{
				list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(answersEntity.Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusAnswerLink, answersEntity.SkillChecksDC, TooltipHelper.GetLinkTooltipTemplate));
			}
		}
		IEnumerable<TextMeshProUGUI> enumerable = from n in m_DialogNotifications.GetAllNotifications()
			where n.gameObject.activeSelf
			select n;
		if (!enumerable.Empty() && m_DialogNotifications.gameObject.activeSelf && !base.IsShowHistory.Value)
		{
			foreach (TextMeshProUGUI item in enumerable)
			{
				list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(item, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusHistoryLink, TooltipHelper.GetLinkTooltipTemplate));
			}
		}
		if (list.Count > 0)
		{
			m_GlossaryNavigationBehavior.AddEntities(list);
		}
		m_HasGlossaryPoints.Value = list.Count > 0;
		return list.FirstItem();
	}

	private void OnFocusHistoryLink(string key)
	{
		if (!(GamePad.Instance.CurrentInputLayer.ContextName != "DialogGlossary"))
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_FirstGlossaryFocus.ShowLinkTooltip(key);
			}, 1);
		}
	}

	private void OnFocusCueLink(string key, List<SkillCheckResult> skillCheckResults)
	{
		if (!(GamePad.Instance.CurrentInputLayer.ContextName != "DialogGlossary"))
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_FirstGlossaryFocus.ShowLinkTooltip(key, null, skillCheckResults);
			}, 1);
		}
	}

	private void OnFocusAnswerLink(string key, List<SkillCheckDC> skillCheckDcs)
	{
		if (!(GamePad.Instance.CurrentInputLayer.ContextName != "DialogGlossary"))
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_FirstGlossaryFocus.ShowLinkTooltip(key, skillCheckDcs);
			}, 1);
		}
	}

	protected override void OnCloseGlossaryMode()
	{
		base.OnCloseGlossaryMode();
		CloseGlossary(default(InputActionEventData));
	}

	private void SwitchHistory(InputActionEventData data)
	{
		SwitchHistory();
	}

	private void OnGlossaryFocusChanged(IConsoleEntity focus)
	{
		if (focus == null)
		{
			return;
		}
		RectTransform rect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
		bool num = ((!base.IsShowHistory.Value) ? (!m_CuesScrollRect.IsInViewport(rect)) : (!m_CuesHistoryScrollRect.IsInViewport(rect)));
		Action action = delegate
		{
			if (!base.IsShowHistory.Value)
			{
				m_CuesScrollRect.SnapToCenter(rect);
			}
			else
			{
				m_CuesHistoryScrollRect.SnapToCenter(rect);
			}
		};
		if (num)
		{
			action();
		}
	}

	public void OnShowEscMenu(InputActionEventData data)
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	private void ScrollMenu(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_AnswersScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
