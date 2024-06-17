using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog.Console;

public class SpaceEventConsoleView : SpaceEventBaseView<DialogAnswerConsoleView, DialogSystemAnswerConsoleView>
{
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	private FloatConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	private InputLayer m_GlossaryInputLayer;

	private BoolReactiveProperty m_HasGlossaryPoints = new BoolReactiveProperty();

	private BoolReactiveProperty m_GlossaryMode = new BoolReactiveProperty();

	private IDisposable m_Disposable;

	private ReactiveProperty<DialogAnswerConsoleView> m_CurrentAnswer = new ReactiveProperty<DialogAnswerConsoleView>();

	protected override void CreateNavigation()
	{
		base.CreateNavigation();
		NavigationBehaviour.FocusOnFirstValidEntity();
		CalculateGlossary();
		AddDisposable(m_ConsoleHintsWidget);
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
	}

	private void OnGlossaryFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform rectTransform = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			if (!m_ScrollRect.IsInViewport(rectTransform))
			{
				m_ScrollRect.ScrollToRectCenter(rectTransform, rectTransform);
			}
		}
	}

	private void ShowGlossary(InputActionEventData data)
	{
		CalculateGlossary();
		if (m_AnswerView.IsBinded)
		{
			m_AnswerView.gameObject.SetActive(value: false);
		}
		m_GlossaryMode.Value = true;
		m_Disposable = GamePad.Instance.PushLayer(m_GlossaryInputLayer);
		m_GlossaryNavigationBehavior.FocusOnFirstValidEntity();
		if (m_CurrentAnswer.Value != null)
		{
			m_CurrentAnswer.Value.ShowAnswerHint(invert: false);
		}
		if (m_SystemAnswerView != null)
		{
			m_SystemAnswerView.ShowAnswerHint(value: false);
		}
	}

	private void CloseGlossary(InputActionEventData data)
	{
		m_GlossaryNavigationBehavior.UnFocusCurrentEntity();
		if (m_AnswerView.IsBinded)
		{
			m_AnswerView.gameObject.SetActive(value: true);
		}
		m_Disposable?.Dispose();
		m_Disposable = null;
		TooltipHelper.HideTooltip();
		m_GlossaryMode.Value = false;
		if (m_CurrentAnswer.Value != null)
		{
			m_CurrentAnswer.Value.ShowAnswerHint(invert: false);
		}
		if (m_SystemAnswerView != null)
		{
			m_SystemAnswerView.ShowAnswerHint(value: false);
		}
		NavigationBehaviour?.FocusOnFirstValidEntity();
	}

	private void OnFocusCueLink(string key, List<SkillCheckResult> skillCheckResults)
	{
		m_FirstGlossaryFocus.ShowLinkTooltip(key, null, skillCheckResults, new TooltipConfig
		{
			InfoCallConsoleMethod = InfoCallConsoleMethod.ShortRightStickButton
		});
	}

	private void OnFocusAnswerLink(string key, List<SkillCheckDC> skillCheckDcs)
	{
		m_FirstGlossaryFocus.ShowLinkTooltip(key, skillCheckDcs, null, new TooltipConfig
		{
			InfoCallConsoleMethod = InfoCallConsoleMethod.ShortRightStickButton
		});
	}

	private void CalculateGlossary()
	{
		m_GlossaryNavigationBehavior.Clear();
		List<IFloatConsoleNavigationEntity> list = TMPLinkNavigationGenerator.GenerateEntityList(m_CueView.Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusCueLink, m_CueView.SkillChecks, TooltipHelper.GetLinkTooltipTemplate);
		if (!m_Answers.Empty())
		{
			foreach (DialogAnswerConsoleView answer in m_Answers)
			{
				List<IFloatConsoleNavigationEntity> collection = TMPLinkNavigationGenerator.GenerateEntityList(answer.Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusAnswerLink, answer.SkillChecksDC, TooltipHelper.GetLinkTooltipTemplate);
				list.AddRange(collection);
			}
		}
		m_GlossaryNavigationBehavior.AddEntities(list);
		m_HasGlossaryPoints.Value = list.Any();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		CloseGlossary(default(InputActionEventData));
		m_Disposable?.Dispose();
		m_Disposable = null;
	}
}
