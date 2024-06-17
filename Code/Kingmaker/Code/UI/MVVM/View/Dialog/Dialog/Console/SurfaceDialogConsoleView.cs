using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Dialog.SurfaceDialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.GameModes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog.Console;

[RequireComponent(typeof(DialogColorsConfig))]
public class SurfaceDialogConsoleView : SurfaceDialogBaseView<DialogAnswerConsoleView>
{
	[Header("Console Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private ConsoleHint m_CuesScroll;

	[SerializeField]
	private ConsoleHint m_AnswersScroll;

	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	[SerializeField]
	private ConsoleHint m_ShowVotesHint;

	[SerializeField]
	private ConsoleHint m_CloseVotesHint;

	[SerializeField]
	private ConsoleHint m_ShowSpeakerPortraitHint;

	[SerializeField]
	private ConsoleHint m_ShowAnswererPortraitHint;

	[Header("Navigations")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	private FloatConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	private InputLayer m_GlossaryInputLayer;

	private readonly BoolReactiveProperty m_HasGlossaryPoints = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_GlossaryMode = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanScroll = new BoolReactiveProperty();

	private IDisposable m_GlossaryDisposable;

	private readonly ReactiveProperty<DialogAnswerConsoleView> m_CurrentAnswer = new ReactiveProperty<DialogAnswerConsoleView>();

	private TooltipConfig m_CuesTooltipConfig;

	private TooltipConfig m_AnswersTooltipConfig;

	private const string GlossaryInputLayerContextName = "DialogGlossary";

	private InputLayer m_VotesInputLayer;

	private GridConsoleNavigationBehaviour m_VotesNavigationBehavior;

	private readonly BoolReactiveProperty m_VotesMode = new BoolReactiveProperty();

	private const string VotesInputLayerContextName = "DialogVotes";

	public override void Initialize()
	{
		base.Initialize();
		m_FirstGlossaryFocus?.gameObject.SetActive(value: false);
		m_SecondGlossaryFocus?.gameObject.SetActive(value: false);
		m_CuesTooltipConfig = new TooltipConfig
		{
			InfoCallConsoleMethod = InfoCallConsoleMethod.ShortRightStickButton,
			TooltipPlace = m_CuesTooltipPlace,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(0.5f, 0f),
				new Vector2(0.5f, 0.05f),
				new Vector2(0.5f, 0.1f),
				new Vector2(0.5f, 0.15f),
				new Vector2(0.5f, 0.2f),
				new Vector2(0.5f, 0.25f)
			}
		};
		m_AnswersTooltipConfig = new TooltipConfig
		{
			InfoCallConsoleMethod = InfoCallConsoleMethod.ShortRightStickButton,
			TooltipPlace = m_AnswersTooltipPlace,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(0.5f, 0f)
			}
		};
	}

	protected override void CreateNavigation()
	{
		base.CreateNavigation();
		if (!IsCargoRewardsOpen)
		{
			NavigationBehaviour.FocusOnEntityManual(Answers.FirstItem());
		}
		CalculateGlossary();
		AddDisposable(m_ConsoleHintsWidget);
		AddDisposable((from x in MainThreadDispatcher.UpdateAsObservable()
			select m_SpeakerScrollRect.verticalNormalizedPosition < 0.001f into x
			where m_CanScroll.Value != x
			select x).Throttle(TimeSpan.FromSeconds(0.20000000298023224)).Subscribe(delegate(bool value)
		{
			m_CanScroll.Value = value;
		}));
	}

	protected override void OnFocusChanged(IConsoleEntity focus)
	{
		base.OnFocusChanged(focus);
		m_CurrentAnswer.Value = focus as DialogAnswerConsoleView;
	}

	protected override void CreateInput()
	{
		base.CreateInput();
		AddDisposable(m_GlossaryNavigationBehavior = new FloatConsoleNavigationBehaviour(m_NavigationParameters));
		m_GlossaryInputLayer = m_GlossaryNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "DialogGlossary"
		});
		AddDisposable(m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Subscribe(OnGlossaryFocusChanged));
		AddDisposable(m_ConsoleHintsWidget.BindHint(InputLayer.AddButton(ShowGlossary, 11, m_HasGlossaryPoints), UIStrings.Instance.Dialog.OpenGlossary));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_GlossaryInputLayer.AddButton(CloseGlossary, 9, m_GlossaryMode), UIStrings.Instance.Dialog.CloseGlossary));
		AddDisposable(m_ConsoleHintsWidget.BindHint(InputLayer.AddButton(delegate
		{
			m_SpeakerScrollRect.ScrollToBottom();
		}, 19, m_CanScroll, InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.Dialog.ScrollToNew));
		AddDisposable(m_ShowVotesHint.Bind(InputLayer.AddButton(delegate
		{
			ShowVotes();
		}, 19, VotesIsActive, InputActionEventType.ButtonJustReleased)));
		m_ShowVotesHint.SetLabel(UIStrings.Instance.Dialog.ShowVotes);
		AddDisposable(InputLayer.AddButton(delegate
		{
			OnShowEscMenu();
		}, 16, InputActionEventType.ButtonJustReleased));
		if ((bool)m_CuesScroll)
		{
			AddDisposable(m_CuesScroll.BindCustomAction(1, InputLayer));
		}
		if ((bool)m_AnswersScroll)
		{
			AddDisposable(m_AnswersScroll.BindCustomAction(3, InputLayer));
		}
		AddDisposable(m_ConsoleHintsWidget.CreateCustomHint(2, InputLayer, UIStrings.Instance.ActionTexts.Rotate));
		AddDisposable(InputLayer.AddAxis2D(OnRightStick, 2, 3, repeat: false));
		AddVotesInput();
	}

	private void AddVotesInput()
	{
		AddDisposable(m_VotesNavigationBehavior = new GridConsoleNavigationBehaviour());
		m_VotesInputLayer = m_VotesNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "DialogVotes"
		});
		AddDisposable(m_CloseVotesHint.Bind(m_VotesInputLayer.AddButton(delegate
		{
			CloseVotes();
		}, 19, m_VotesMode, InputActionEventType.ButtonJustReleased)));
		m_CloseVotesHint.SetLabel(UIStrings.Instance.Dialog.HideVotes);
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
		list.Add(m_TopDialogVotesBlock.FocusButton);
		list.AddRange(Answers.Select((DialogAnswerConsoleView a) => a.DialogVotesBlock.FocusButton));
		list.Add(m_BottomDialogVotesBlock.FocusButton);
		List<OwlcatSelectable> list2 = (from block in list.OfType<OwlcatSelectable>()
			where block.IsValid() && m_AnswerScrollRect.VisibleVerticalPosition((RectTransform)block.gameObject.transform, -10f) == 0
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

	private void OnRightStick(InputActionEventData data, Vector2 vector)
	{
		if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
		{
			if (vector.x > 0f)
			{
				CameraRig.Instance.RotateLeft();
			}
			else
			{
				CameraRig.Instance.RotateRight();
			}
		}
		else
		{
			m_SpeakerScrollRect.Scroll(vector.y, smooth: true);
		}
	}

	private void OnGlossaryFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform target = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			if (!m_SpeakerScrollRect.IsInViewport(target))
			{
				m_SpeakerScrollRect.SnapToCenter(target);
			}
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

	private IFloatConsoleNavigationEntity CalculateGlossary()
	{
		m_GlossaryNavigationBehavior.Clear();
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		try
		{
			list = TMPLinkNavigationGenerator.GenerateEntityList(m_CueView.Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusCueLink, m_CueView.SkillChecks, TooltipHelper.GetLinkTooltipTemplate);
			for (int num = HistoryEntities.Count - 1; num >= 0; num--)
			{
				list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(HistoryEntities[num].Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusHistoryLink, TooltipHelper.GetLinkTooltipTemplate));
			}
			if (!Answers.Empty())
			{
				foreach (DialogAnswerConsoleView answer in Answers)
				{
					answer.SetTooltipConfig(m_AnswersTooltipConfig);
					list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(answer.Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusAnswerLink, answer.SkillChecksDC, TooltipHelper.GetLinkTooltipTemplate));
				}
			}
			IEnumerable<TextMeshProUGUI> enumerable = from n in m_DialogNotifications.GetAllNotifications()
				where n.gameObject.activeSelf
				select n;
			if (!enumerable.Empty())
			{
				foreach (TextMeshProUGUI item in enumerable)
				{
					list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(item, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusHistoryLink, TooltipHelper.GetLinkTooltipTemplate));
				}
			}
		}
		catch (Exception arg)
		{
			PFLog.UI.Error($"Can't create glossary navigation: {arg}");
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
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_FirstGlossaryFocus.ShowLinkTooltip(key, null, null, m_CuesTooltipConfig);
		}, 1);
	}

	private void OnFocusCueLink(string key, List<SkillCheckResult> skillCheckResults)
	{
		if (!(GamePad.Instance.CurrentInputLayer.ContextName != "DialogGlossary"))
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_FirstGlossaryFocus.ShowLinkTooltip(key, null, skillCheckResults, m_CuesTooltipConfig);
			}, 1);
		}
	}

	private void OnFocusAnswerLink(string key, List<SkillCheckDC> skillCheckDcs)
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_FirstGlossaryFocus.ShowLinkTooltip(key, skillCheckDcs, null, m_AnswersTooltipConfig);
		}, 1);
	}

	protected override void OnPartsUpdating()
	{
		TooltipHelper.HideTooltip();
	}

	private void OnShowEscMenu()
	{
		if (!Game.Instance.Player.Tutorial.HasShownData && !(Game.Instance.CurrentMode == GameModeType.GameOver))
		{
			base.ViewModel.HandleShowEscMenu();
		}
	}

	protected override void OnCloseGlossaryMode()
	{
		base.OnCloseGlossaryMode();
		CloseGlossary(default(InputActionEventData));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		CloseGlossary(default(InputActionEventData));
		m_GlossaryDisposable?.Dispose();
		m_GlossaryDisposable = null;
	}
}
