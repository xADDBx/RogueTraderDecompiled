using DG.Tweening;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.PC;

public class TooltipPCView : TooltipBaseView
{
	protected override void BindViewImplementation()
	{
		if (!base.ViewModel.IsComparative)
		{
			AddDisposable(DelayedInvoker.InvokeInTime(delegate
			{
				base.BindViewImplementation();
			}, 0.2f));
		}
		else
		{
			base.BindViewImplementation();
		}
	}

	protected override void Show()
	{
		AddDisposable(DelayedInvoker.InvokeInTime(delegate
		{
			SetHeight();
			if (!base.ViewModel.IsComparative)
			{
				UIUtility.SetPopupWindowPosition((RectTransform)base.transform, base.ViewModel.OwnerTransform, Vector2.zero, base.ViewModel.PriorityPivots);
				m_ShowTween = base.CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
				{
					UISounds.Instance.Sounds.Tooltip.TooltipShow.Play();
					IsShowed = true;
				}).SetUpdate(isIndependentUpdate: true);
			}
		}, 0.1f));
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
