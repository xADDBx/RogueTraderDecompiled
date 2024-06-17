using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Common;

public class ParallaxPointerWatcher : MonoBehaviour, IPointerMoveHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	private Vector2 m_WatcherBasePositionRectTransformPosition;

	private Vector2 m_BasePositionRectTransformPosition;

	[SerializeField]
	private float m_ParallaxGainX = 2f;

	[SerializeField]
	private float m_ParallaxGainY = 2f;

	[SerializeField]
	private DOTweenAnimation m_TweenVisualAnimation;

	[SerializeField]
	private float m_Smoothness = 0.1f;

	private Tweener m_MoveTween;

	[SerializeField]
	private float m_BackTime = 0.1f;

	public void Awake()
	{
		m_WatcherBasePositionRectTransformPosition = (m_BasePositionRectTransformPosition = ((RectTransform)base.transform).anchoredPosition);
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		float x = m_WatcherBasePositionRectTransformPosition.x + (eventData.position.x - ((RectTransform)base.transform).rect.width) * m_ParallaxGainX * 0.01f;
		float y = m_WatcherBasePositionRectTransformPosition.x + (eventData.position.y - ((RectTransform)base.transform).rect.height / 2f) * m_ParallaxGainY * 0.01f;
		Vector2 anchoredPosition = ((RectTransform)base.transform).anchoredPosition;
		((RectTransform)base.transform).anchoredPosition = Vector2.Lerp(anchoredPosition, new Vector2(x, y), Time.unscaledDeltaTime * m_Smoothness);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_WatcherBasePositionRectTransformPosition = ((RectTransform)base.transform).anchoredPosition;
		m_TweenVisualAnimation.DOPause();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		((RectTransform)base.transform).DOAnchorPos(m_BasePositionRectTransformPosition, m_BackTime).SetAutoKill(autoKillOnCompletion: true).OnKill(delegate
		{
			m_TweenVisualAnimation.DORewindAndPlayNext();
		});
	}
}
