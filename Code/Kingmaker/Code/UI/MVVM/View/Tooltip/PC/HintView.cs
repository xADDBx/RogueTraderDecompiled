using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.PC;

public class HintView : ViewBase<HintVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private string m_BindingTextFormat = " ({0})";

	private Dictionary<string, string> m_TextTags = new Dictionary<string, string> { { "<separator>", "<align=\"center\">------------------</align>" } };

	private CanvasGroup m_CanvasGroup;

	private Tweener m_ShowTween;

	private RectTransform m_RectTransform;

	private RectTransform m_ParentRectTransform;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_CanvasGroup = base.gameObject.EnsureComponent<CanvasGroup>();
		m_RectTransform = (RectTransform)base.transform;
		m_ParentRectTransform = (RectTransform)base.transform.parent;
	}

	protected override void BindViewImplementation()
	{
		SetHintText();
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.UpdateAsObservable(), delegate
		{
			UpdateHintPosition();
		}));
		AddDisposable(DelayedInvoker.InvokeInTime(delegate
		{
			DelayedBind();
		}, 0.2f));
	}

	private void DelayedBind()
	{
		base.gameObject.SetActive(value: true);
		m_CanvasGroup.alpha = 0f;
		DelayedInvoker.InvokeInTime(delegate
		{
			if (base.gameObject.activeInHierarchy)
			{
				UpdateHintPosition();
				m_ShowTween = m_CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
				{
					UISounds.Instance.Sounds.Hint.HintShow.Play();
				}).SetUpdate(isIndependentUpdate: true);
			}
		}, 0.1f);
	}

	private void SetHintText()
	{
		StringBuilder stringBuilder = new StringBuilder(base.ViewModel.Text);
		if (!string.IsNullOrEmpty(base.ViewModel.BindingText))
		{
			stringBuilder.Append(string.Format(m_BindingTextFormat, base.ViewModel.BindingText));
		}
		ApplyTextTags(stringBuilder);
		m_Label.text = stringBuilder.ToString();
		m_Label.color = base.ViewModel.Color;
	}

	private void ApplyTextTags(StringBuilder sb)
	{
		foreach (KeyValuePair<string, string> textTag in m_TextTags)
		{
			sb.Replace(textTag.Key, textTag.Value);
		}
	}

	private void UpdateHintPosition()
	{
		Vector2 cursorPosition = Game.Instance.CursorController.CursorPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRectTransform, cursorPosition, UICamera.Instance, out var localPoint);
		m_RectTransform.localPosition = UIUtility.LimitPositionRectInRect(localPoint, m_ParentRectTransform, m_RectTransform);
	}

	protected override void DestroyViewImplementation()
	{
		if (m_CanvasGroup.alpha == 1f)
		{
			UISounds.Instance.Sounds.Hint.HintHide.Play();
		}
		m_CanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		StopAllCoroutines();
		m_ShowTween?.Kill();
		m_ShowTween = null;
	}
}
