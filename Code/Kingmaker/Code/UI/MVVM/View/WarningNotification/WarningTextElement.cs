using System;
using DG.Tweening;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.WarningNotification;

[Serializable]
public class WarningTextElement : IWarningElement
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private CanvasGroup m_Container;

	public void Initialize()
	{
		m_Container.alpha = 0f;
	}

	public void SetText(string label)
	{
		m_Label.SetText(label);
	}

	public void ShowSequenceCanvasGroupFadeAnimation(float showHideTime, float stayOnScreenTime, bool withSound = true)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(m_Container.DOFade(1f, showHideTime));
		sequence.Append(m_Container.DOFade(1f, stayOnScreenTime));
		sequence.Append(m_Container.DOFade(0f, showHideTime));
		sequence.Play().SetUpdate(isIndependentUpdate: true);
		if (withSound)
		{
			UISounds.Instance.Sounds.GreenMessageLine.GreenMessageLineShow.Play();
			DelayedInvoker.InvokeInTime(delegate
			{
				UISounds.Instance.Sounds.GreenMessageLine.GreenMessageLineHide.Play();
			}, showHideTime + stayOnScreenTime);
		}
	}
}
