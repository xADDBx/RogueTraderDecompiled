using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Canvases;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.SafeZone;

public class SafeZoneComponent : MonoBehaviour, ISafeZoneUIHandler, ISubscriber
{
	[SerializeField]
	private bool m_ApplySize = true;

	[SerializeField]
	private bool m_ApplyPosition = true;

	private float m_InitWidth;

	private float m_InitHeight;

	private Vector2 m_InitAnchoredPosition;

	private RectTransform RectTransform => base.transform as RectTransform;

	private Rect Rect => RectTransform.rect;

	private void Awake()
	{
		EventBus.Subscribe(this);
		SaveInitialValues();
		ApplySafeZone();
	}

	private void OnDestroy()
	{
		EventBus.Unsubscribe(this);
	}

	private void ApplySafeZone()
	{
		float num = 1f - (float)(int)SettingsRoot.Display.SafeZoneOffset / 100f;
		MainCanvas instance = MainCanvas.Instance;
		if (!(instance == null))
		{
			Rect rect = instance.RectTransform.rect;
			float num2 = rect.width * num;
			float num3 = rect.height * num;
			float num4 = Rect.width;
			float num5 = Rect.height;
			if (num4 > num2 && m_ApplySize)
			{
				RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num2);
				num4 = num2;
			}
			if (num5 > num3 && m_ApplySize)
			{
				RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num3);
				num5 = num3;
			}
			if (m_ApplyPosition)
			{
				Vector3 position = RectTransform.position;
				Vector3 lossyScale = RectTransform.lossyScale;
				float num6 = position.x / lossyScale.x + Rect.center.x;
				float num7 = position.y / lossyScale.y + Rect.center.y;
				float num8 = (num2 - num4) / 2f;
				float num9 = (num3 - num5) / 2f;
				float x = Mathf.Min(num8 - num6, 0f) + Mathf.Max(0f - num8 - num6, 0f);
				float y = Mathf.Min(num9 - num7, 0f) + Mathf.Max(0f - num9 - num7, 0f);
				Vector2 anchoredPosition = RectTransform.anchoredPosition;
				anchoredPosition += new Vector2(x, y);
				RectTransform.anchoredPosition = anchoredPosition;
			}
		}
	}

	private void SaveInitialValues()
	{
		if (m_ApplySize)
		{
			m_InitWidth = Rect.width;
			m_InitHeight = Rect.height;
		}
		if (m_ApplyPosition)
		{
			m_InitAnchoredPosition = RectTransform.anchoredPosition;
		}
	}

	private void SetInitialValues()
	{
		if (m_ApplySize)
		{
			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_InitWidth);
			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_InitHeight);
		}
		if (m_ApplyPosition)
		{
			RectTransform.anchoredPosition = m_InitAnchoredPosition;
		}
	}

	public void OnSafeZoneChanged()
	{
		SetInitialValues();
		ApplySafeZone();
	}
}
