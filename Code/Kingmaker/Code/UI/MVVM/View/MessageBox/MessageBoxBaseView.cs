using System;
using System.Text;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.UI.TMPExtention;
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

	private Tweener m_ProgressTweener;

	public virtual void Initialize()
	{
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
		m_ProgressTweener?.Kill();
		m_ProgressTweener = null;
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		base.gameObject.SetActive(value: false);
	}

	private void SetProgressBar()
	{
		m_ProgressParent.gameObject.SetActive(base.ViewModel.IsProgressBar.Value);
		m_ProgressTransform.sizeDelta = new Vector2(0f, m_ProgressTransform.rect.height);
		m_PercentText.text = Mathf.CeilToInt(0f) + "%";
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
			m_PercentText.text = Mathf.CeilToInt(virtualProgress * 100f) + "%";
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

	protected abstract void SetAcceptInteractable(bool interactable);

	protected abstract void SetAcceptText(string acceptText);

	protected abstract void SetDeclineText(string declineText);

	protected abstract void BindTextField();

	protected abstract void DestroyTextField();

	protected abstract void BindProgressBar();
}
