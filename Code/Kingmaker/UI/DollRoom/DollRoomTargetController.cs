using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Workarounds;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.UI.DollRoom;

public class DollRoomTargetController : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
	[FormerlySerializedAs("Character")]
	public Transform Target;

	[SerializeField]
	private RawImage m_RawImage;

	[SerializeField]
	private CanvasScalerWorkaround m_CanvasScaler;

	public float MaxAngularSpeed = 360f;

	private Quaternion m_StartRotation;

	private Vector3 m_Rotation;

	private PointerEventData m_EventData;

	public CanvasScalerWorkaround CanvasScaler
	{
		get
		{
			return m_CanvasScaler;
		}
		set
		{
			m_CanvasScaler = value;
		}
	}

	public RawImage RawImage => m_RawImage;

	public Vector2 GetRawImageSize(CanvasScalerWorkaround defaultCanvasScaler)
	{
		RectTransform rectTransform = (RectTransform)m_RawImage.transform;
		CanvasScalerWorkaround canvasScalerWorkaround = ((m_CanvasScaler != null) ? m_CanvasScaler : defaultCanvasScaler);
		if (rectTransform.rect.size.x == 0f)
		{
			return canvasScalerWorkaround.referenceResolution * canvasScalerWorkaround.scaleFactor;
		}
		return rectTransform.rect.size * canvasScalerWorkaround.scaleFactor;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		m_StartRotation = Target.rotation;
		m_Rotation = m_StartRotation.eulerAngles;
		EventBus.RaiseEvent(delegate(IDollCharacterDragUIHandler h)
		{
			h.StartDrag();
		});
		if ((bool)DollCamera.Current && eventData != null && eventData.button == PointerEventData.InputButton.Right)
		{
			DollCamera.Current.BeginZoom();
		}
		m_EventData = eventData;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		m_EventData = null;
		EventBus.RaiseEvent(delegate(IDollCharacterDragUIHandler h)
		{
			h.EndDrag();
		});
		if ((bool)DollCamera.Current)
		{
			DollCamera.Current.EndZoom();
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!ApplicationFocusEvents.DollRoomDisabled && m_EventData != null)
		{
			m_EventData = null;
			EventBus.RaiseEvent(delegate(IDollCharacterDragUIHandler h)
			{
				h.EndDrag();
			});
			if ((bool)DollCamera.Current)
			{
				DollCamera.Current.EndZoom();
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			float amount = (0f - Mathf.Min(Mathf.Abs(eventData.delta.x * 0.4f), MaxAngularSpeed * Time.unscaledDeltaTime)) * Mathf.Sign(eventData.delta.x);
			Rotate(amount);
		}
	}

	public void Update()
	{
		if (m_EventData != null && (bool)DollCamera.Current && m_EventData.button == PointerEventData.InputButton.Right)
		{
			DollCamera.Current.Zoom(m_EventData.delta.y * 0.1f);
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		Zoom(eventData.scrollDelta.y);
	}

	public void Zoom(float value)
	{
		if ((bool)DollCamera.Current)
		{
			DollCamera.Current.Zoom(value);
		}
	}

	public void Rotate(float amount)
	{
		m_Rotation.y = amount;
		Target.Rotate(m_Rotation);
	}

	public void ZoomMin()
	{
		if ((bool)DollCamera.Current)
		{
			DollCamera.Current.ZoomMin();
		}
	}

	public void ZoomMax()
	{
		if ((bool)DollCamera.Current)
		{
			DollCamera.Current.ZoomMax();
		}
	}
}
