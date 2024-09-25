using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rewired.Demos;

[AddComponentMenu("")]
[RequireComponent(typeof(Image))]
public class TouchJoystickExample : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
{
	public bool allowMouseControl = true;

	public int radius = 50;

	private Vector2 origAnchoredPosition;

	private Vector3 origWorldPosition;

	private Vector2 origScreenResolution;

	private ScreenOrientation origScreenOrientation;

	[NonSerialized]
	private bool hasFinger;

	[NonSerialized]
	private int lastFingerId;

	public Vector2 position { get; private set; }

	private void Start()
	{
		if (SystemInfo.deviceType == DeviceType.Handheld)
		{
			allowMouseControl = false;
		}
		StoreOrigValues();
	}

	private void Update()
	{
		if ((float)Screen.width != origScreenResolution.x || (float)Screen.height != origScreenResolution.y || Screen.orientation != origScreenOrientation)
		{
			Restart();
			StoreOrigValues();
		}
	}

	private void Restart()
	{
		hasFinger = false;
		(base.transform as RectTransform).anchoredPosition = origAnchoredPosition;
		position = Vector2.zero;
	}

	private void StoreOrigValues()
	{
		origAnchoredPosition = (base.transform as RectTransform).anchoredPosition;
		origWorldPosition = base.transform.position;
		origScreenResolution = new Vector2(Screen.width, Screen.height);
		origScreenOrientation = Screen.orientation;
	}

	private void UpdateValue(Vector3 value)
	{
		Vector3 vector = origWorldPosition - value;
		vector.y = 0f - vector.y;
		vector /= (float)radius;
		position = new Vector2(0f - vector.x, vector.y);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (!hasFinger && (allowMouseControl || !IsMousePointerId(eventData.pointerId)))
		{
			hasFinger = true;
			lastFingerId = eventData.pointerId;
		}
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (eventData.pointerId == lastFingerId && (allowMouseControl || !IsMousePointerId(eventData.pointerId)))
		{
			Restart();
		}
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if (hasFinger && eventData.pointerId == lastFingerId)
		{
			Vector3 vector = new Vector3(eventData.position.x - origWorldPosition.x, eventData.position.y - origWorldPosition.y);
			vector = Vector3.ClampMagnitude(vector, radius);
			Vector3 value = origWorldPosition + vector;
			base.transform.position = value;
			UpdateValue(value);
		}
	}

	private static bool IsMousePointerId(int id)
	{
		if (id != -1 && id != -2)
		{
			return id == -3;
		}
		return true;
	}
}
