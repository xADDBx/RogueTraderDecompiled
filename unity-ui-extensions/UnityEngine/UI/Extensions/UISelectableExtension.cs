using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/UI Selectable Extension")]
[RequireComponent(typeof(Selectable))]
public class UISelectableExtension : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	[Serializable]
	public class UIButtonEvent : UnityEvent<PointerEventData.InputButton>
	{
	}

	[Tooltip("Event that fires when a button is initially pressed down")]
	public UIButtonEvent OnButtonPress;

	[Tooltip("Event that fires when a button is released")]
	public UIButtonEvent OnButtonRelease;

	[Tooltip("Event that continually fires while a button is held down")]
	public UIButtonEvent OnButtonHeld;

	private bool _pressed;

	private PointerEventData _heldEventData;

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (OnButtonPress != null)
		{
			OnButtonPress.Invoke(eventData.button);
		}
		_pressed = true;
		_heldEventData = eventData;
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (OnButtonRelease != null)
		{
			OnButtonRelease.Invoke(eventData.button);
		}
		_pressed = false;
		_heldEventData = null;
	}

	private void Update()
	{
		if (_pressed && OnButtonHeld != null)
		{
			OnButtonHeld.Invoke(_heldEventData.button);
		}
	}

	public void TestClicked()
	{
	}

	public void TestPressed()
	{
	}

	public void TestReleased()
	{
	}

	public void TestHold()
	{
	}

	private void OnDisable()
	{
		_pressed = false;
	}
}
