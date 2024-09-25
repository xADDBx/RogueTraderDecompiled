using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.InputSystems;

[AddComponentMenu("Event/PS4 Input Module")]
public class PS4InputModule : PointerInputModule
{
	private float m_PrevActionTime;

	private Vector2 m_LastMoveVector;

	private int m_ConsecutiveMoveCount;

	private bool m_IsRepeating;

	private Vector2 m_LastMousePosition;

	private Vector2 m_MousePosition;

	[Tooltip("If enabled then the touch pad button acts as a UI submit button.")]
	public bool m_EnableTouchPadButton = true;

	[Tooltip("Input manager horizontal axis to use for UI navigation.")]
	public string m_HorizontalAxis = "Horizontal";

	[Tooltip("Input manager vertical axis to use for UI navigation.")]
	public string m_VerticalAxis = "Vertical";

	[Tooltip("Input manager button to use for submit.")]
	public string m_SubmitButton = "Submit";

	[Tooltip("Input manager button to use for cancel.")]
	public string m_CancelButton = "Cancel";

	[Tooltip("String appended to the above input manager axis names, this can be used to switch input from one user to another.")]
	public string m_AxisNameSuffix = "_0";

	[Tooltip("Repeat speed in actions per second.")]
	public float m_RepeatSpeed = 4f;

	[Tooltip("Repeat delay in seconds.")]
	public float m_RepeatDelay = 0.25f;

	protected PS4InputModule()
	{
	}

	public override void UpdateModule()
	{
		m_LastMousePosition = m_MousePosition;
		m_MousePosition = Input.mousePosition;
	}

	public override bool IsModuleSupported()
	{
		return true;
	}

	private bool UseFakeInput()
	{
		return !Input.touchSupported;
	}

	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule())
		{
			return false;
		}
		if (UseFakeInput())
		{
			if (Input.GetMouseButtonDown(0))
			{
				return true;
			}
			if ((m_MousePosition - m_LastMousePosition).sqrMagnitude > 0f)
			{
				return true;
			}
		}
		Input.GetButtonDown(m_SubmitButton + m_AxisNameSuffix);
		return false | Input.GetButtonDown(m_CancelButton + m_AxisNameSuffix) | !Mathf.Approximately(Input.GetAxisRaw(m_HorizontalAxis + m_AxisNameSuffix), 0f) | !Mathf.Approximately(Input.GetAxisRaw(m_VerticalAxis + m_AxisNameSuffix), 0f);
	}

	public override void ActivateModule()
	{
		base.ActivateModule();
		m_MousePosition = Input.mousePosition;
		m_LastMousePosition = Input.mousePosition;
		GameObject gameObject = base.eventSystem.currentSelectedGameObject;
		if (gameObject == null)
		{
			gameObject = base.eventSystem.firstSelectedGameObject;
		}
		base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
	}

	public override void DeactivateModule()
	{
		base.DeactivateModule();
		ClearSelection();
	}

	private void ProcessTouchEvents()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (touch.phase == (TouchPhase)7)
			{
				touch.phase = TouchPhase.Began;
			}
			bool pressed;
			bool released;
			PointerEventData touchPointerEventData = GetTouchPointerEventData(touch, out pressed, out released);
			ProcessTouchPress(touchPointerEventData, pressed, released);
			if (!released)
			{
				ProcessMove(touchPointerEventData);
				ProcessDrag(touchPointerEventData);
			}
			else
			{
				RemovePointerData(touchPointerEventData);
			}
		}
	}

	private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
	{
		GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
		if (pressed)
		{
			pointerEvent.eligibleForClick = true;
			pointerEvent.delta = Vector2.zero;
			pointerEvent.dragging = false;
			pointerEvent.useDragThreshold = true;
			pointerEvent.pressPosition = pointerEvent.position;
			pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
			DeselectIfSelectionChangedAndValid(gameObject, pointerEvent, ignoreNullSelection: true);
			if (pointerEvent.pointerEnter != gameObject)
			{
				HandlePointerExitAndEnter(pointerEvent, gameObject);
				pointerEvent.pointerEnter = gameObject;
			}
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == pointerEvent.lastPress)
			{
				if (unscaledTime - pointerEvent.clickTime < 0.3f)
				{
					int clickCount = pointerEvent.clickCount + 1;
					pointerEvent.clickCount = clickCount;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				pointerEvent.clickTime = unscaledTime;
			}
			else
			{
				base.eventSystem.SetSelectedGameObject(gameObject2, pointerEvent);
				pointerEvent.clickCount = 1;
			}
			pointerEvent.pointerPress = gameObject2;
			pointerEvent.rawPointerPress = gameObject;
			pointerEvent.clickTime = unscaledTime;
			pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (pointerEvent.pointerDrag != null)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (released)
		{
			ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (pointerEvent.pointerPress == eventHandler && pointerEvent.eligibleForClick)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
			}
			else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.dropHandler);
			}
			pointerEvent.eligibleForClick = false;
			pointerEvent.pointerPress = null;
			pointerEvent.rawPointerPress = null;
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.dragging = false;
			pointerEvent.pointerDrag = null;
			if (pointerEvent.pointerDrag != null)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.pointerDrag = null;
			ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
			pointerEvent.pointerEnter = null;
		}
	}

	private void FakeTouches()
	{
		MouseButtonEventData eventData = GetMousePointerEventData(0).GetButtonState(PointerEventData.InputButton.Left).eventData;
		if (eventData.PressedThisFrame())
		{
			eventData.buttonData.delta = Vector2.zero;
		}
		ProcessTouchPress(eventData.buttonData, eventData.PressedThisFrame(), eventData.ReleasedThisFrame());
		if (Input.GetMouseButton(0))
		{
			ProcessMove(eventData.buttonData);
			ProcessDrag(eventData.buttonData);
		}
	}

	public override void Process()
	{
		bool flag = SendUpdateEventToSelectedObject();
		if (base.eventSystem.sendNavigationEvents)
		{
			if (!flag)
			{
				flag |= SendMoveEventToSelectedObject();
			}
			if (!flag)
			{
				SendSubmitEventToSelectedObject();
			}
		}
		if (m_EnableTouchPadButton)
		{
			if (UseFakeInput())
			{
				FakeTouches();
			}
			else
			{
				ProcessTouchEvents();
			}
		}
	}

	protected bool SendSubmitEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		if (Input.GetButtonDown(m_SubmitButton + m_AxisNameSuffix))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
		}
		if (Input.GetButtonDown(m_CancelButton + m_AxisNameSuffix))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
		}
		return baseEventData.used;
	}

	private Vector2 GetRawMoveVector()
	{
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxisRaw(m_HorizontalAxis + m_AxisNameSuffix);
		zero.y = Input.GetAxisRaw(m_VerticalAxis + m_AxisNameSuffix);
		if (Input.GetButtonDown(m_HorizontalAxis + m_AxisNameSuffix))
		{
			if (zero.x < 0f)
			{
				zero.x = -1f;
			}
			if (zero.x > 0f)
			{
				zero.x = 1f;
			}
		}
		if (Input.GetButtonDown(m_VerticalAxis + m_AxisNameSuffix))
		{
			if (zero.y < 0f)
			{
				zero.y = -1f;
			}
			if (zero.y > 0f)
			{
				zero.y = 1f;
			}
		}
		return zero;
	}

	protected bool SendMoveEventToSelectedObject()
	{
		float unscaledTime = Time.unscaledTime;
		Vector2 rawMoveVector = GetRawMoveVector();
		if (Mathf.Approximately(rawMoveVector.x, 0f) && Mathf.Approximately(rawMoveVector.y, 0f))
		{
			m_PrevActionTime = unscaledTime;
			m_LastMoveVector = rawMoveVector;
			m_ConsecutiveMoveCount = 0;
			m_IsRepeating = false;
			return false;
		}
		bool flag = Vector2.Dot(rawMoveVector, m_LastMoveVector) > 0f;
		if (m_IsRepeating)
		{
			bool flag2 = Input.GetButtonDown(m_HorizontalAxis + m_AxisNameSuffix) || Input.GetButtonDown(m_VerticalAxis + m_AxisNameSuffix);
			if (!flag2)
			{
				flag2 = ((!flag || m_ConsecutiveMoveCount != 1) ? (unscaledTime > m_PrevActionTime + 1f / m_RepeatSpeed) : (unscaledTime > m_PrevActionTime + m_RepeatDelay));
			}
			if (!flag2)
			{
				return false;
			}
		}
		AxisEventData axisEventData = GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0f);
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
		if (!flag)
		{
			m_ConsecutiveMoveCount = 0;
		}
		m_ConsecutiveMoveCount++;
		m_PrevActionTime = unscaledTime;
		m_LastMoveVector = rawMoveVector;
		m_IsRepeating = true;
		return axisEventData.used;
	}

	protected bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	protected void DeselectIfSelectionChangedAndValid(GameObject currentOverGo, BaseEventData pointerEvent, bool ignoreNullSelection)
	{
		GameObject eventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
		if ((!ignoreNullSelection || !(eventHandler == null)) && eventHandler != base.eventSystem.currentSelectedGameObject)
		{
			base.eventSystem.SetSelectedGameObject(null, pointerEvent);
		}
	}
}
