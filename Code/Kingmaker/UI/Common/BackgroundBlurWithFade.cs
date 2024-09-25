using Kingmaker.UI.Common.Animations;
using UnityEngine;

namespace Kingmaker.UI.Common;

public class BackgroundBlurWithFade : BackgroundBlur
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void OnEnable()
	{
		if ((bool)m_FadeAnimator)
		{
			m_FadeAnimator.OnAppearEvent += EnableBlur;
			m_FadeAnimator.OnDisappearEvent += DisableBlur;
		}
		else
		{
			base.OnEnable();
		}
	}

	protected override void OnDisable()
	{
		if ((bool)m_FadeAnimator)
		{
			m_FadeAnimator.OnAppearEvent -= EnableBlur;
			m_FadeAnimator.OnDisappearEvent -= DisableBlur;
		}
		else
		{
			base.OnDisable();
		}
	}

	private void EnableBlur()
	{
		SetBlurState(state: true);
	}

	private void DisableBlur()
	{
		SetBlurState(state: false);
	}
}
