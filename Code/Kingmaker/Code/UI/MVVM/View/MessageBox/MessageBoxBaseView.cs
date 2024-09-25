using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.UI.TMPExtention;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.MessageBox;

public abstract class MessageBoxBaseView : ViewBase<MessageBoxVM>
{
	[SerializeField]
	private TextMeshProUGUI m_MessageText;

	[SerializeField]
	private TMPLinkHandler m_LinkHandler;

	[Header("Progress Bar")]
	[SerializeField]
	private RectTransform m_ProgressParent;

	[SerializeField]
	private RectTransform m_ProgressTransform;

	[SerializeField]
	private TextMeshProUGUI m_PercentText;

	[Header("ScrollBar")]
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("ResetAnimation")]
	[SerializeField]
	private List<CanvasGroup> m_CanvasesResetAnimation;

	[Header("Checkbox")]
	[SerializeField]
	protected OwlcatToggle m_DontShowToggle;

	[SerializeField]
	protected TextMeshProUGUI m_DontShowLabel;

	private Tweener m_ProgressTweener;

	public virtual void Initialize()
	{
		ResetCanvasesAnimation();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		m_MessageText.text = base.ViewModel.MessageText;
		ScrollToTop();
		AddDisposable(base.ViewModel.WaitTime.Subscribe(delegate(int value)
		{
			StringBuilder stringBuilder = new StringBuilder(base.ViewModel.AcceptText);
			if (value > 0)
			{
				stringBuilder.Append($" ({value})");
			}
			SetAcceptText(stringBuilder.ToString());
		}));
		SetDeclineText(base.ViewModel.DeclineText);
		AddDisposable(base.ViewModel.InputText.CombineLatest(base.ViewModel.WaitTime, delegate(string s, int i)
		{
			bool flag = i == 0;
			if (flag && base.ViewModel.BoxType == DialogMessageBoxBase.BoxType.TextField)
			{
				flag = base.ViewModel.InputText.Value.Length > 0;
			}
			return flag;
		}).Subscribe(SetAcceptInteractable));
		AddDisposable(m_LinkHandler.OnClickAsObservable().Subscribe(delegate(Tuple<PointerEventData, TMP_LinkInfo> value)
		{
			base.ViewModel.OnLinkInvoke(value.Item2);
		}));
		BindTextField();
		SetCheckbox();
		SetProgressBar();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			if (!base.ViewModel.IsProgressBar.Value)
			{
				base.ViewModel.OnDeclinePressed();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		DestroyTextField();
		if (m_DontShowToggle != null && m_DontShowToggle.IsOn.Value)
		{
			base.ViewModel.DontShowAgainInvoke();
		}
		m_DontShowToggle.Or(null)?.Set(value: false);
		m_ProgressTweener?.Kill();
		m_ProgressTweener = null;
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		ResetCanvasesAnimation();
		base.gameObject.SetActive(value: false);
	}

	private void SetCheckbox()
	{
		if (!(m_DontShowToggle == null))
		{
			m_DontShowToggle.gameObject.SetActive(base.ViewModel.IsCheckbox.Value);
			m_DontShowToggle.Set(value: false);
			m_DontShowLabel.text = UIStrings.Instance.Tutorial.DontShowThisTutorial;
			UISounds.Instance.SetClickAndHoverSound(m_DontShowToggle.ConsoleEntityProxy as OwlcatMultiButton, UISounds.ButtonSoundsEnum.NoSound);
		}
	}

	private void SetProgressBar()
	{
		m_ProgressParent.gameObject.SetActive(base.ViewModel.IsProgressBar.Value);
		m_ProgressTransform.sizeDelta = new Vector2(0f, m_ProgressTransform.rect.height);
		m_PercentText.text = UIConfig.Instance.PercentHelper.AddPercentTo(Mathf.CeilToInt(0f));
		if (!base.ViewModel.IsProgressBar.Value)
		{
			return;
		}
		BindProgressBar();
		if (base.ViewModel.LoadingProgress != null)
		{
			AddDisposable(base.ViewModel.LoadingProgressCloseTrigger.Subscribe(delegate
			{
				base.ViewModel.OnAcceptPressed();
			}));
			AddDisposable(base.ViewModel.LoadingProgress.Subscribe(SetLoadingProgress));
		}
	}

	private void SetLoadingProgress(float virtualProgress)
	{
		virtualProgress = Mathf.Clamp01(virtualProgress);
		float progressWidth = m_ProgressParent.rect.width;
		float startValue = ((m_ProgressTransform.rect.width > 0f && progressWidth > 0f) ? (m_ProgressTransform.rect.width / progressWidth) : 0f);
		m_ProgressTweener?.Kill();
		m_ProgressTweener = DOTween.To(delegate
		{
			m_ProgressTransform.sizeDelta = new Vector2(virtualProgress * progressWidth, m_ProgressTransform.rect.height);
			m_PercentText.text = UIConfig.Instance.PercentHelper.AddPercentTo(Mathf.CeilToInt(virtualProgress * 100f));
		}, startValue, virtualProgress, 0.5f).SetEase(Ease.Linear);
	}

	private void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}

	protected virtual void OnTextInputChanged(string value)
	{
		base.ViewModel.InputText.Value = value;
	}

	private void ResetCanvasesAnimation()
	{
		if (!m_CanvasesResetAnimation.Any())
		{
			return;
		}
		m_CanvasesResetAnimation.ForEach(delegate(CanvasGroup canvasGroup)
		{
			if (!(canvasGroup == null))
			{
				canvasGroup.alpha = 0f;
			}
		});
	}

	protected abstract void SetAcceptInteractable(bool interactable);

	protected abstract void SetAcceptText(string acceptText);

	protected abstract void SetDeclineText(string declineText);

	protected abstract void BindTextField();

	protected abstract void DestroyTextField();

	protected abstract void BindProgressBar();
}
