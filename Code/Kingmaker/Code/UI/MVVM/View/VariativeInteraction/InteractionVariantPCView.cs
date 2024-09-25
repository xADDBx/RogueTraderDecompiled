using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.VariativeInteraction;

public class InteractionVariantPCView : InteractionVariantView
{
	[Header("Hint")]
	[SerializeField]
	private FadeAnimator m_HintAnimator;

	[SerializeField]
	private TextMeshProUGUI m_HintText;

	protected override void BindViewImplementation()
	{
		m_HintAnimator.CanvasGroup.alpha = 0f;
		base.BindViewImplementation();
		AddDisposable(ObservableExtensions.Subscribe(m_Button.OnSingleLeftClickAsObservable(), delegate
		{
			base.ViewModel.Interact();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_Button.OnSingleLeftClickNotInteractableAsObservable(), delegate
		{
			base.ViewModel.Interact();
		}));
		m_HintText.text = GetHint();
		AddDisposable(this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			ShowHint();
		}));
		AddDisposable(this.OnPointerExitAsObservable().Subscribe(delegate
		{
			HideHint();
		}));
	}

	private void ShowHint()
	{
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		if (requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0)
		{
			m_HintAnimator.AppearAnimation();
		}
	}

	private void HideHint()
	{
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		if (requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0)
		{
			m_HintAnimator.DisappearAnimation();
		}
	}

	private string GetHint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.ViewModel.ResourceName + "\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.HasResourceCount.Text}: {base.ViewModel.ResourceCount}\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.RequiredResourceCount.Text}: {base.ViewModel.RequiredResourceCount}\n");
		return stringBuilder.ToString();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		HideHint();
	}
}
