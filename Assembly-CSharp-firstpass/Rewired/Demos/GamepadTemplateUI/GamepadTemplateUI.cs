using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos.GamepadTemplateUI;

public class GamepadTemplateUI : MonoBehaviour
{
	private class Stick
	{
		private RectTransform _transform;

		private Vector2 _origPosition;

		private int _xAxisElementId = -1;

		private int _yAxisElementId = -1;

		public Vector2 position
		{
			get
			{
				if (!(_transform != null))
				{
					return Vector2.zero;
				}
				return _transform.anchoredPosition - _origPosition;
			}
			set
			{
				if (!(_transform == null))
				{
					_transform.anchoredPosition = _origPosition + value;
				}
			}
		}

		public Stick(RectTransform transform, int xAxisElementId, int yAxisElementId)
		{
			if (!(transform == null))
			{
				_transform = transform;
				_origPosition = _transform.anchoredPosition;
				_xAxisElementId = xAxisElementId;
				_yAxisElementId = yAxisElementId;
			}
		}

		public void Reset()
		{
			if (!(_transform == null))
			{
				_transform.anchoredPosition = _origPosition;
			}
		}

		public bool ContainsElement(int elementId)
		{
			if (_transform == null)
			{
				return false;
			}
			if (elementId != _xAxisElementId)
			{
				return elementId == _yAxisElementId;
			}
			return true;
		}

		public void SetAxisPosition(int elementId, float value)
		{
			if (!(_transform == null))
			{
				Vector2 vector = position;
				if (elementId == _xAxisElementId)
				{
					vector.x = value;
				}
				else if (elementId == _yAxisElementId)
				{
					vector.y = value;
				}
				position = vector;
			}
		}
	}

	private class UIElement
	{
		public int id;

		public ControllerUIElement element;

		public UIElement(int id, ControllerUIElement element)
		{
			this.id = id;
			this.element = element;
		}
	}

	private const float stickRadius = 20f;

	public int playerId;

	[SerializeField]
	private RectTransform leftStick;

	[SerializeField]
	private RectTransform rightStick;

	[SerializeField]
	private ControllerUIElement leftStickX;

	[SerializeField]
	private ControllerUIElement leftStickY;

	[SerializeField]
	private ControllerUIElement leftStickButton;

	[SerializeField]
	private ControllerUIElement rightStickX;

	[SerializeField]
	private ControllerUIElement rightStickY;

	[SerializeField]
	private ControllerUIElement rightStickButton;

	[SerializeField]
	private ControllerUIElement actionBottomRow1;

	[SerializeField]
	private ControllerUIElement actionBottomRow2;

	[SerializeField]
	private ControllerUIElement actionBottomRow3;

	[SerializeField]
	private ControllerUIElement actionTopRow1;

	[SerializeField]
	private ControllerUIElement actionTopRow2;

	[SerializeField]
	private ControllerUIElement actionTopRow3;

	[SerializeField]
	private ControllerUIElement leftShoulder;

	[SerializeField]
	private ControllerUIElement leftTrigger;

	[SerializeField]
	private ControllerUIElement rightShoulder;

	[SerializeField]
	private ControllerUIElement rightTrigger;

	[SerializeField]
	private ControllerUIElement center1;

	[SerializeField]
	private ControllerUIElement center2;

	[SerializeField]
	private ControllerUIElement center3;

	[SerializeField]
	private ControllerUIElement dPadUp;

	[SerializeField]
	private ControllerUIElement dPadRight;

	[SerializeField]
	private ControllerUIElement dPadDown;

	[SerializeField]
	private ControllerUIElement dPadLeft;

	private UIElement[] _uiElementsArray;

	private Dictionary<int, ControllerUIElement> _uiElements = new Dictionary<int, ControllerUIElement>();

	private IList<ControllerTemplateElementTarget> _tempTargetList = new List<ControllerTemplateElementTarget>(2);

	private Stick[] _sticks;

	private Player player => ReInput.players.GetPlayer(playerId);

	private void Awake()
	{
		_uiElementsArray = new UIElement[23]
		{
			new UIElement(0, leftStickX),
			new UIElement(1, leftStickY),
			new UIElement(17, leftStickButton),
			new UIElement(2, rightStickX),
			new UIElement(3, rightStickY),
			new UIElement(18, rightStickButton),
			new UIElement(4, actionBottomRow1),
			new UIElement(5, actionBottomRow2),
			new UIElement(6, actionBottomRow3),
			new UIElement(7, actionTopRow1),
			new UIElement(8, actionTopRow2),
			new UIElement(9, actionTopRow3),
			new UIElement(14, center1),
			new UIElement(15, center2),
			new UIElement(16, center3),
			new UIElement(19, dPadUp),
			new UIElement(20, dPadRight),
			new UIElement(21, dPadDown),
			new UIElement(22, dPadLeft),
			new UIElement(10, leftShoulder),
			new UIElement(11, leftTrigger),
			new UIElement(12, rightShoulder),
			new UIElement(13, rightTrigger)
		};
		for (int i = 0; i < _uiElementsArray.Length; i++)
		{
			_uiElements.Add(_uiElementsArray[i].id, _uiElementsArray[i].element);
		}
		_sticks = new Stick[2]
		{
			new Stick(leftStick, 0, 1),
			new Stick(rightStick, 2, 3)
		};
		ReInput.ControllerConnectedEvent += OnControllerConnected;
		ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
	}

	private void Start()
	{
		if (ReInput.isReady)
		{
			DrawLabels();
		}
	}

	private void OnDestroy()
	{
		ReInput.ControllerConnectedEvent -= OnControllerConnected;
		ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
	}

	private void Update()
	{
		if (ReInput.isReady)
		{
			DrawActiveElements();
		}
	}

	private void DrawActiveElements()
	{
		for (int i = 0; i < _uiElementsArray.Length; i++)
		{
			_uiElementsArray[i].element.Deactivate();
		}
		for (int j = 0; j < _sticks.Length; j++)
		{
			_sticks[j].Reset();
		}
		IList<InputAction> actions = ReInput.mapping.Actions;
		for (int k = 0; k < actions.Count; k++)
		{
			ActivateElements(player, actions[k].id);
		}
	}

	private void ActivateElements(Player player, int actionId)
	{
		float axis = player.GetAxis(actionId);
		if (axis == 0f)
		{
			return;
		}
		IList<InputActionSourceData> currentInputSources = player.GetCurrentInputSources(actionId);
		for (int i = 0; i < currentInputSources.Count; i++)
		{
			InputActionSourceData inputActionSourceData = currentInputSources[i];
			IGamepadTemplate template = inputActionSourceData.controller.GetTemplate<IGamepadTemplate>();
			if (template == null)
			{
				continue;
			}
			template.GetElementTargets(inputActionSourceData.actionElementMap, _tempTargetList);
			for (int j = 0; j < _tempTargetList.Count; j++)
			{
				ControllerTemplateElementTarget controllerTemplateElementTarget = _tempTargetList[j];
				int id = controllerTemplateElementTarget.element.id;
				ControllerUIElement controllerUIElement = _uiElements[id];
				if (controllerTemplateElementTarget.elementType == ControllerTemplateElementType.Axis)
				{
					controllerUIElement.Activate(axis);
				}
				else if (controllerTemplateElementTarget.elementType == ControllerTemplateElementType.Button && (player.GetButton(actionId) || player.GetNegativeButton(actionId)))
				{
					controllerUIElement.Activate(1f);
				}
				GetStick(id)?.SetAxisPosition(id, axis * 20f);
			}
		}
	}

	private void DrawLabels()
	{
		for (int i = 0; i < _uiElementsArray.Length; i++)
		{
			_uiElementsArray[i].element.ClearLabels();
		}
		IList<InputAction> actions = ReInput.mapping.Actions;
		for (int j = 0; j < actions.Count; j++)
		{
			DrawLabels(player, actions[j]);
		}
	}

	private void DrawLabels(Player player, InputAction action)
	{
		Controller firstControllerWithTemplate = player.controllers.GetFirstControllerWithTemplate<IGamepadTemplate>();
		if (firstControllerWithTemplate == null)
		{
			return;
		}
		IGamepadTemplate template = firstControllerWithTemplate.GetTemplate<IGamepadTemplate>();
		ControllerMap map = player.controllers.maps.GetMap(firstControllerWithTemplate, "Default", "Default");
		if (map != null)
		{
			for (int i = 0; i < _uiElementsArray.Length; i++)
			{
				ControllerUIElement element = _uiElementsArray[i].element;
				int id = _uiElementsArray[i].id;
				IControllerTemplateElement element2 = template.GetElement(id);
				DrawLabel(element, action, map, template, element2);
			}
		}
	}

	private void DrawLabel(ControllerUIElement uiElement, InputAction action, ControllerMap controllerMap, IControllerTemplate template, IControllerTemplateElement element)
	{
		if (element.source == null)
		{
			return;
		}
		if (element.source.type == ControllerTemplateElementSourceType.Axis)
		{
			IControllerTemplateAxisSource controllerTemplateAxisSource = element.source as IControllerTemplateAxisSource;
			ActionElementMap firstElementMapWithElementTarget;
			if (controllerTemplateAxisSource.splitAxis)
			{
				firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(controllerTemplateAxisSource.positiveTarget, action.id, skipDisabledMaps: true);
				if (firstElementMapWithElementTarget != null)
				{
					uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, AxisRange.Positive);
				}
				firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(controllerTemplateAxisSource.negativeTarget, action.id, skipDisabledMaps: true);
				if (firstElementMapWithElementTarget != null)
				{
					uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, AxisRange.Negative);
				}
				return;
			}
			firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(controllerTemplateAxisSource.fullTarget, action.id, skipDisabledMaps: true);
			if (firstElementMapWithElementTarget != null)
			{
				uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, AxisRange.Full);
				return;
			}
			firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(new ControllerElementTarget(controllerTemplateAxisSource.fullTarget)
			{
				axisRange = AxisRange.Positive
			}, action.id, skipDisabledMaps: true);
			if (firstElementMapWithElementTarget != null)
			{
				uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, AxisRange.Positive);
			}
			firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(new ControllerElementTarget(controllerTemplateAxisSource.fullTarget)
			{
				axisRange = AxisRange.Negative
			}, action.id, skipDisabledMaps: true);
			if (firstElementMapWithElementTarget != null)
			{
				uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, AxisRange.Negative);
			}
		}
		else if (element.source.type == ControllerTemplateElementSourceType.Button)
		{
			IControllerTemplateButtonSource controllerTemplateButtonSource = element.source as IControllerTemplateButtonSource;
			ActionElementMap firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(controllerTemplateButtonSource.target, action.id, skipDisabledMaps: true);
			if (firstElementMapWithElementTarget != null)
			{
				uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, AxisRange.Full);
			}
		}
	}

	private Stick GetStick(int elementId)
	{
		for (int i = 0; i < _sticks.Length; i++)
		{
			if (_sticks[i].ContainsElement(elementId))
			{
				return _sticks[i];
			}
		}
		return null;
	}

	private void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		DrawLabels();
	}

	private void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		DrawLabels();
	}
}
