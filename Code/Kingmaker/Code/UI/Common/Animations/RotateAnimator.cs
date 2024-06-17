using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.Code.UI.Common.Animations;

public class RotateAnimator : MonoBehaviour, IUIAnimator
{
	[SerializeField]
	private float m_AppearRotationZ;

	[SerializeField]
	private float m_AppearTime = 0.2f;

	[SerializeField]
	private Ease m_AppearAnimCurve = Ease.Linear;

	private Vector3 m_AppearRotation;

	[SerializeField]
	private float m_DisappearRotationZ;

	[SerializeField]
	private float m_DisappearTime = 0.2f;

	[SerializeField]
	private Ease m_DisappearAnimCurve = Ease.Linear;

	private Vector3 m_DisappearRotation;

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
		if (!m_isInit)
		{
			m_AppearRotation = new Vector3(0f, 0f, m_AppearRotationZ);
			m_DisappearRotation = new Vector3(0f, 0f, m_DisappearRotationZ);
			RectTransform.localEulerAngles = m_DisappearRotation;
			m_isInit = true;
		}
	}

	public void AppearAnimation(UnityAction action = null)
	{
		Initialize();
		RectTransform.DOLocalRotate(m_AppearRotation, m_AppearTime).SetEase(m_AppearAnimCurve).SetUpdate(isIndependentUpdate: true);
		action?.Invoke();
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		Initialize();
		RectTransform.DOLocalRotate(m_DisappearRotation, m_DisappearTime).SetEase(m_DisappearAnimCurve).SetUpdate(isIndependentUpdate: true);
		action?.Invoke();
	}

	public void PlayAnimation(bool value, UnityAction action = null)
	{
		if (value)
		{
			AppearAnimation(action);
		}
		else
		{
			DisappearAnimation(action);
		}
	}
}
