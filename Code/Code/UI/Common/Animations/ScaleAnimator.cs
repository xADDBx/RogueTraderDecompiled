using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UI.Common.Animations;

public class ScaleAnimator : MonoBehaviour, IUIAnimator
{
	[SerializeField]
	private Vector3 m_BaseScale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private Vector3 m_Scale = new Vector3(1.2f, 1.2f, 1.2f);

	[SerializeField]
	private float m_ScaleTime = 0.2f;

	[SerializeField]
	private int m_LoopsCount = -1;

	private Tweener m_Tweener;

	private RectTransform m_RectTransform;

	private bool m_isInit;

	private RectTransform RectTransform
	{
		get
		{
			RectTransform obj = m_RectTransform ?? GetComponent<RectTransform>();
			RectTransform result = obj;
			m_RectTransform = obj;
			return result;
		}
	}

	public void Initialize()
	{
		if (!m_isInit && !(RectTransform == null))
		{
			RectTransform.localScale = m_BaseScale;
			m_isInit = true;
		}
	}

	public void AppearAnimation(UnityAction action = null)
	{
		Initialize();
		if (m_Tweener == null)
		{
			CreateTweener();
		}
		m_Tweener.OnComplete(delegate
		{
			action?.Invoke();
		});
		m_Tweener.Restart();
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		Initialize();
		m_Tweener.Pause();
		action?.Invoke();
	}

	private void CreateTweener()
	{
		m_Tweener = RectTransform.DOScale(m_Scale, m_ScaleTime).SetEase(Ease.InOutSine).SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: false)
			.Pause()
			.SetLoops(m_LoopsCount, LoopType.Yoyo);
	}

	public void DestroyViewImplementation()
	{
		m_Tweener.Kill();
		m_Tweener = null;
		m_isInit = false;
	}

	public void OnDestroy()
	{
		DestroyViewImplementation();
	}
}
