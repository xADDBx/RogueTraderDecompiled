using DG.Tweening;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

public class WindowAnimator : MonoBehaviour, IUIAnimator
{
	private CanvasGroup m_CanvasGroup;

	public int Angle = 15;

	private Tweener m_AppearFade;

	private Tweener m_AppearRotation;

	[SerializeField]
	[UsedImplicitly]
	private float m_AppearTime = 0.2f;

	[SerializeField]
	[UsedImplicitly]
	private Ease m_AppearAnimCurve = Ease.Linear;

	private Tweener m_DisappearFade;

	private Tweener m_DisappearRotation;

	[SerializeField]
	[UsedImplicitly]
	private float m_DisappearTime = 0.2f;

	[SerializeField]
	[UsedImplicitly]
	private Ease m_DisappearAnimCurve = Ease.Linear;

	private UnityAction m_AppearAction;

	private UnityAction m_DisappearAction;

	private bool m_isInit;

	private CanvasGroup CanvasGroup => m_CanvasGroup = (m_CanvasGroup ? m_CanvasGroup : base.gameObject.EnsureComponent<CanvasGroup>());

	public void Initialize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			CanvasGroup.alpha = 0f;
			CanvasGroup.blocksRaycasts = false;
			m_AppearRotation = base.transform.DOLocalRotate(Vector3.zero, m_AppearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_DisappearAnimCurve)
				.SetUpdate(isIndependentUpdate: true);
			m_AppearFade = CanvasGroup.DOFade(1f, m_AppearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_AppearAnimCurve)
				.SetUpdate(isIndependentUpdate: true)
				.OnComplete(delegate
				{
					CanvasGroup.blocksRaycasts = true;
					CanvasGroup.alpha = 1f;
					m_AppearAction?.Invoke();
				});
			m_DisappearRotation = base.transform.DOLocalRotate(Vector3.down * Angle, m_DisappearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_DisappearAnimCurve)
				.SetUpdate(isIndependentUpdate: true);
			m_DisappearFade = CanvasGroup.DOFade(0f, m_DisappearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_DisappearAnimCurve)
				.SetUpdate(isIndependentUpdate: true)
				.OnComplete(delegate
				{
					base.gameObject.SetActive(value: false);
					CanvasGroup.alpha = 0f;
					m_DisappearAction?.Invoke();
				});
			m_isInit = true;
		}
	}

	public void AppearAnimation(UnityAction action = null)
	{
		base.gameObject.SetActive(value: true);
		if (!base.gameObject.activeInHierarchy)
		{
			CanvasGroup.alpha = 1f;
			CanvasGroup.blocksRaycasts = true;
			action?.Invoke();
			return;
		}
		if (!m_isInit)
		{
			Initialize();
		}
		if (m_AppearFade == null || m_DisappearFade == null || m_AppearRotation == null || m_DisappearRotation == null)
		{
			m_isInit = false;
			Initialize();
		}
		m_AppearAction = action;
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.Set(0f, Angle, 0f);
		base.transform.eulerAngles = eulerAngles;
		base.gameObject.SetActive(value: true);
		if (m_DisappearFade.IsPlaying())
		{
			m_DisappearFade.Pause();
		}
		if (m_DisappearRotation.IsPlaying())
		{
			m_DisappearRotation.Pause();
		}
		m_AppearFade.ChangeStartValue(CanvasGroup.alpha);
		m_AppearFade.Play();
		m_AppearRotation.ChangeStartValue(base.transform.eulerAngles);
		m_AppearRotation.Play();
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			CanvasGroup.alpha = 0f;
			return;
		}
		if (!m_isInit)
		{
			Initialize();
		}
		if (m_AppearFade == null || m_DisappearFade == null || m_AppearRotation == null || m_DisappearRotation == null)
		{
			m_isInit = false;
			Initialize();
		}
		m_DisappearAction = action;
		CanvasGroup.blocksRaycasts = false;
		if (m_AppearFade.IsPlaying())
		{
			m_AppearFade.Pause();
		}
		if (m_AppearRotation.IsPlaying())
		{
			m_AppearRotation.Pause();
		}
		m_DisappearFade.ChangeStartValue(CanvasGroup.alpha);
		m_DisappearFade.Play();
		m_DisappearRotation.ChangeStartValue(base.transform.eulerAngles);
		m_DisappearRotation.Play();
	}

	public void OnDisable()
	{
		m_AppearFade?.Kill();
		m_DisappearFade?.Kill();
		m_AppearRotation?.Kill();
		m_DisappearRotation?.Kill();
		m_isInit = false;
	}
}
