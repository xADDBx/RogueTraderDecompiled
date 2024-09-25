using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console;

public class TooltipConsoleView : TooltipBaseView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_InteractionHint;

	protected override void Show()
	{
		AddDisposable(DelayedInvoker.InvokeInFrames(delegate
		{
			SetHeight();
			if (!base.ViewModel.IsComparative)
			{
				UIUtility.SetPopupWindowPosition((RectTransform)base.transform, base.ViewModel.OwnerTransform, (base.ViewModel.PriorityPivots == null) ? new Vector2(0f, 25f) : Vector2.zero, base.ViewModel.PriorityPivots);
				m_ShowTween = base.CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
				{
					UISounds.Instance.Sounds.Tooltip.TooltipShow.Play();
					IsShowed = true;
				}).SetUpdate(isIndependentUpdate: true);
			}
		}, 1));
	}

	protected override string GetInteractionHint()
	{
		return UIStrings.Instance.CommonTexts.Expand;
	}

	protected override void SetupInteractionHint()
	{
		m_InteractionHint.SetActive(base.ViewModel.IsPrimitive);
		if (base.ViewModel.IsPrimitive)
		{
			return;
		}
		InputLayer currentInputLayer = GamePad.Instance.CurrentInputLayer;
		if (base.ViewModel.HasScroll)
		{
			AddDisposable(currentInputLayer.AddAxis(base.Scroll, 3));
		}
		switch (base.ViewModel.InfoCallConsoleMethod)
		{
		case InfoCallConsoleMethod.LongShortRightStickButton:
			AddDisposable(m_InteractionHint.Bind(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 19)));
			AddDisposable(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 19, InputActionEventType.ButtonJustLongPressed));
			break;
		case InfoCallConsoleMethod.ShortRightStickButton:
			AddDisposable(m_InteractionHint.Bind(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 19)));
			break;
		case InfoCallConsoleMethod.LongRightStickButton:
			AddDisposable(m_InteractionHint.Bind(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 19, InputActionEventType.ButtonJustLongPressed)));
			break;
		case InfoCallConsoleMethod.FunkAdditionalButton:
			AddDisposable(m_InteractionHint.Bind(currentInputLayer.AddButton(delegate
			{
				ShowInfo();
			}, 17)));
			break;
		default:
			m_InteractionHint.SetActive(state: false);
			break;
		}
		m_InteractionHint.SetLabel(GetInteractionHint());
		m_HintContainer.gameObject.SetActive(value: true);
	}

	private void ShowInfo()
	{
		if (base.ViewModel.IsGlossary && base.ViewModel.MainTemplate is TooltipTemplateGlossary { GlossaryEntry: not null } tooltipTemplateGlossary)
		{
			TooltipHelper.ShowGlossaryInfo(tooltipTemplateGlossary, base.ViewModel.OwnerNavigationBehaviour);
		}
		else if (base.ViewModel.Templates != null)
		{
			TooltipHelper.ShowInfo(base.ViewModel.Templates, base.ViewModel.OwnerNavigationBehaviour);
		}
		else
		{
			TooltipHelper.ShowInfo(base.ViewModel.MainTemplate, base.ViewModel.OwnerNavigationBehaviour, base.ViewModel.ShouldNotHideLittleTooltip);
		}
	}

	protected override void DestroyViewImplementation()
	{
		if (IsShowed)
		{
			UISounds.Instance.Sounds.Tooltip.TooltipHide.Play();
		}
		IsShowed = false;
		base.DestroyViewImplementation();
	}
}
