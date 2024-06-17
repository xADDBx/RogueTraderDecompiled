using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Subtitle;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Subtitle;

public class SubtitleView : ViewBase<SubtitleVM>
{
	[SerializeField]
	private TextMeshProUGUI m_SubtitleText;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	public void Initialize()
	{
		base.gameObject.SetActive(value: true);
		m_CanvasGroup.alpha = 0f;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.BarkText.Subscribe(delegate(string value)
		{
			m_CanvasGroup.DOFade((value != string.Empty) ? 1 : 0, 0.2f).SetUpdate(isIndependentUpdate: true);
			m_SubtitleText.text = value;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
