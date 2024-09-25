using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Code.UI.Common.Animations;

public class RankEntryAnimator : MonoBehaviour
{
	[SerializeField]
	private ScaleAnimator m_ScaleAnimator;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	private bool m_IsPlaying;

	public void PlayOnce()
	{
		m_ScaleAnimator.Or(null)?.PlayOnce();
		m_FadeAnimator.Or(null)?.PlayOnce();
	}

	public void StartAnimation()
	{
		if (!m_IsPlaying)
		{
			m_IsPlaying = true;
			m_ScaleAnimator.Or(null)?.AppearAnimation();
			m_FadeAnimator.Or(null)?.AppearAnimation();
		}
	}

	public void StopAnimation()
	{
		if (m_IsPlaying)
		{
			m_IsPlaying = false;
			m_ScaleAnimator.Or(null)?.DisappearAnimation();
			m_FadeAnimator.Or(null)?.DisappearAnimation();
		}
	}
}
