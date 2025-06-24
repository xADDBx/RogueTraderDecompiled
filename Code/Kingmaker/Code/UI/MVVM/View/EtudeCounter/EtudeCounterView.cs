using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.EtudeCounter;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.EtudeCounter;

public class EtudeCounterView : ViewBase<EtudeCounterVM>
{
	[SerializeField]
	protected RectTransform m_ContainterTransform;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_LabelRed;

	[SerializeField]
	private CanvasGroup m_CounterContainer;

	[SerializeField]
	private TextMeshProUGUI m_CounterLabel;

	[SerializeField]
	private GameObject m_SliderContainer;

	[SerializeField]
	private Image m_SliderImage;

	[SerializeField]
	private GameObject m_GreenBackground;

	[SerializeField]
	private GameObject m_RedBackground;

	[SerializeField]
	private GameObject m_SuccessIcon;

	[SerializeField]
	private MoveAnimator m_ExtraTextMoveAnimator;

	[FormerlySerializedAs("m_ExtraTextButton")]
	[SerializeField]
	protected OwlcatMultiButton m_SubLabelButton;

	[FormerlySerializedAs("m_ExtraText")]
	[SerializeField]
	private TextMeshProUGUI m_SubLabel;

	[FormerlySerializedAs("m_ExtraButtonUpLabel")]
	[SerializeField]
	private GameObject m_SubLabelButtonUpLabel;

	[FormerlySerializedAs("m_ExtraButtonDownLabel")]
	[SerializeField]
	private GameObject m_SubLabelButtonDownLabel;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		AddDisposable(base.ViewModel.Label.Subscribe(delegate(string text)
		{
			m_Label.text = text;
			m_LabelRed.text = text;
		}));
		AddDisposable(base.ViewModel.Counter.Subscribe(delegate(string text)
		{
			m_CounterLabel.text = text;
		}));
		AddDisposable(base.ViewModel.Progress.Subscribe(delegate(float progress)
		{
			m_SliderImage.fillAmount = progress;
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.CounterChanged, delegate
		{
			UISounds.Instance.Sounds.EtudeCounterSound.EtudeCounterChangeCounter.Play();
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.ProgressChanged, delegate
		{
			UISounds.Instance.Sounds.EtudeCounterSound.EtudeCounterChangeProgress.Play();
		}));
		AddDisposable(base.ViewModel.ShowCounter.CombineLatest(base.ViewModel.IsSystemFailEnabled, base.ViewModel.IsSystemSuccessEnabled, (bool show, bool fail, bool success) => show && !fail && !success).Subscribe(delegate(bool show)
		{
			m_CounterContainer.alpha = (show ? 1f : 0f);
		}));
		AddDisposable(base.ViewModel.ShowProgress.Subscribe(delegate(bool show)
		{
			m_SliderContainer.SetActive(show);
		}));
		AddDisposable(base.ViewModel.IsShowing.Subscribe(delegate(bool isShowing)
		{
			if (isShowing)
			{
				OnShow();
			}
			else
			{
				OnHide();
			}
		}));
		AddDisposable(base.ViewModel.IsSystemFailEnabled.Subscribe(delegate(bool value)
		{
			SetGreenRedState(!value);
			if (value)
			{
				UISounds.Instance.Sounds.EtudeCounterSound.EtudeCounterFail.Play();
			}
		}));
		AddDisposable(base.ViewModel.IsSystemSuccessEnabled.Subscribe(delegate(bool value)
		{
			m_SuccessIcon.gameObject.SetActive(value);
			if (value)
			{
				UISounds.Instance.Sounds.EtudeCounterSound.EtudeCounterSuccess.Play();
			}
		}));
		AddDisposable(base.ViewModel.IsSystemFailEnabled.Or(base.ViewModel.IsSystemSuccessEnabled).Subscribe(delegate(bool value)
		{
			if (value)
			{
				m_ContainterTransform.DOKill();
				m_ContainterTransform.DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).SetEase(Ease.InExpo);
			}
		}));
		AddDisposable(base.ViewModel.SubLabel.Subscribe(delegate(string text)
		{
			m_SubLabel.text = text;
		}));
		AddDisposable(base.ViewModel.ShowSubLabel.Subscribe(delegate(bool value)
		{
			m_SubLabelButton.gameObject.SetActive(value);
			m_ExtraTextMoveAnimator.gameObject.SetActive(value);
		}));
		AddDisposable(base.ViewModel.IsExtraTextShowed.Subscribe(delegate(bool isShowed)
		{
			m_SubLabelButtonUpLabel.gameObject.SetActive(isShowed);
			m_SubLabelButtonDownLabel.gameObject.SetActive(!isShowed);
		}));
		AddDisposable(base.ViewModel.ShowExtraText.Subscribe(ShowExtraText));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetGreenRedState(bool isGreen)
	{
		m_GreenBackground.SetActive(isGreen);
		m_Label.gameObject.SetActive(isGreen);
		m_RedBackground.SetActive(!isGreen);
		m_LabelRed.gameObject.SetActive(!isGreen);
	}

	protected virtual void OnShow()
	{
		UISounds.Instance.Sounds.EtudeCounterSound.EtudeCounterShow.Play();
	}

	protected virtual void OnHide()
	{
		UISounds.Instance.Sounds.EtudeCounterSound.EtudeCounterHide.Play();
	}

	protected void ShowExtraText(bool value)
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_ExtraTextMoveAnimator.PlayAnimation(value);
		}, value ? 1 : 0);
	}
}
