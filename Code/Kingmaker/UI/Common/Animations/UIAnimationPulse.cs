using DG.Tweening;
using UnityEngine;

namespace Kingmaker.UI.Common.Animations;

public class UIAnimationPulse : MonoBehaviour
{
	public Vector3 Scale;

	public float Time;

	private Vector3 m_BasicScale;

	private bool m_IsBusy;

	private bool m_IsInit;

	public void Init()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
			m_BasicScale = base.transform.localScale;
		}
	}

	public void Run()
	{
		if (!m_IsInit)
		{
			Init();
		}
		if (m_IsBusy)
		{
			return;
		}
		m_IsBusy = true;
		base.transform.localScale = Vector3.one;
		base.transform.DOScale(Scale, Time * 2f / 3f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			base.transform.DOScale(m_BasicScale, Time / 3f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				m_IsBusy = false;
			});
		});
	}
}
