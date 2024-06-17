using Kingmaker.Code.UI.MVVM.VM.EtudeCounter;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.EtudeCounter;

public class EtudeCounterView : ViewBase<EtudeCounterVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CounterText.Subscribe(delegate(string text)
		{
			m_Label.text = text;
			if (!string.IsNullOrEmpty(text))
			{
				m_FadeAnimator.AppearAnimation();
			}
			else
			{
				m_FadeAnimator.DisappearAnimation();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
