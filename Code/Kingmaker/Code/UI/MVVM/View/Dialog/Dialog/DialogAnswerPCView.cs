using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

public class DialogAnswerPCView : DialogAnswerBaseView
{
	[SerializeField]
	private float m_DefaultFontSize = 20f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_AnswerText.OnPointerEnterAsObservable().Subscribe(delegate
		{
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}));
		AddDisposable(m_AnswerText.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
		{
			if (data.button == PointerEventData.InputButton.Left)
			{
				Confirm();
			}
		}));
		AddDisposable(m_AnswerText.OnPointerEnterAsObservable().Subscribe(delegate
		{
			if (Focus.State == FocusState.None)
			{
				base.ViewModel?.PingAnswerHover(hover: true);
			}
		}));
		AddDisposable(m_AnswerText.OnPointerExitAsObservable().Subscribe(delegate
		{
			if (Focus.State == FocusState.None)
			{
				base.ViewModel?.PingAnswerHover(hover: false);
			}
		}));
		SetTextFontSize(base.ViewModel.FontSizeMultiplier);
	}

	private void SetTextFontSize(float multiplier)
	{
		m_AnswerText.fontSize = m_DefaultFontSize * multiplier;
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		WidgetFactory.DisposeWidget(this);
	}

	public override void UpdateTextSize(float multiplier)
	{
		SetTextFontSize(multiplier);
		base.UpdateTextSize(multiplier);
	}
}
