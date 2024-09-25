using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class SimpleCombinedKeyboardMouseRemapping : MonoBehaviour
{
	private class Row
	{
		public InputAction action;

		public AxisRange actionRange;

		public Button button;

		public Text text;
	}

	private struct TargetMapping
	{
		public ControllerMap controllerMap;

		public int actionElementMapId;
	}

	private const string category = "Default";

	private const string layout = "Default";

	private const string uiCategory = "UI";

	private InputMapper inputMapper_keyboard = new InputMapper();

	private InputMapper inputMapper_mouse = new InputMapper();

	public GameObject buttonPrefab;

	public GameObject textPrefab;

	public RectTransform fieldGroupTransform;

	public RectTransform actionGroupTransform;

	public Text controllerNameUIText;

	public Text statusUIText;

	private List<Row> rows = new List<Row>();

	private TargetMapping _replaceTargetMapping;

	private Player player => ReInput.players.GetPlayer(0);

	private void OnEnable()
	{
		if (ReInput.isReady)
		{
			inputMapper_keyboard.options.timeout = 5f;
			inputMapper_mouse.options.timeout = 5f;
			inputMapper_mouse.options.ignoreMouseXAxis = true;
			inputMapper_mouse.options.ignoreMouseYAxis = true;
			inputMapper_keyboard.options.allowButtonsOnFullAxisAssignment = false;
			inputMapper_mouse.options.allowButtonsOnFullAxisAssignment = false;
			inputMapper_keyboard.InputMappedEvent += OnInputMapped;
			inputMapper_keyboard.StoppedEvent += OnStopped;
			inputMapper_mouse.InputMappedEvent += OnInputMapped;
			inputMapper_mouse.StoppedEvent += OnStopped;
			InitializeUI();
		}
	}

	private void OnDisable()
	{
		inputMapper_keyboard.Stop();
		inputMapper_mouse.Stop();
		inputMapper_keyboard.RemoveAllEventListeners();
		inputMapper_mouse.RemoveAllEventListeners();
	}

	private void RedrawUI()
	{
		controllerNameUIText.text = "Keyboard/Mouse";
		for (int i = 0; i < rows.Count; i++)
		{
			Row row = rows[i];
			InputAction action = rows[i].action;
			string text = string.Empty;
			int actionElementMapId = -1;
			for (int j = 0; j < 2; j++)
			{
				ControllerType controllerType = ((j != 0) ? ControllerType.Mouse : ControllerType.Keyboard);
				foreach (ActionElementMap item in player.controllers.maps.GetMap(controllerType, 0, "Default", "Default").ElementMapsWithAction(action.id))
				{
					if (item.ShowInField(row.actionRange))
					{
						text = item.elementIdentifierName;
						actionElementMapId = item.id;
						break;
					}
				}
				if (actionElementMapId >= 0)
				{
					break;
				}
			}
			row.text.text = text;
			row.button.onClick.RemoveAllListeners();
			int index = i;
			row.button.onClick.AddListener(delegate
			{
				OnInputFieldClicked(index, actionElementMapId);
			});
		}
	}

	private void ClearUI()
	{
		controllerNameUIText.text = string.Empty;
		for (int i = 0; i < rows.Count; i++)
		{
			rows[i].text.text = string.Empty;
		}
	}

	private void InitializeUI()
	{
		foreach (Transform item in actionGroupTransform)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in fieldGroupTransform)
		{
			Object.Destroy(item2.gameObject);
		}
		foreach (InputAction item3 in ReInput.mapping.ActionsInCategory("Default"))
		{
			if (item3.type == InputActionType.Axis)
			{
				CreateUIRow(item3, AxisRange.Full, item3.descriptiveName);
				CreateUIRow(item3, AxisRange.Positive, (!string.IsNullOrEmpty(item3.positiveDescriptiveName)) ? item3.positiveDescriptiveName : (item3.descriptiveName + " +"));
				CreateUIRow(item3, AxisRange.Negative, (!string.IsNullOrEmpty(item3.negativeDescriptiveName)) ? item3.negativeDescriptiveName : (item3.descriptiveName + " -"));
			}
			else if (item3.type == InputActionType.Button)
			{
				CreateUIRow(item3, AxisRange.Positive, item3.descriptiveName);
			}
		}
		RedrawUI();
	}

	private void CreateUIRow(InputAction action, AxisRange actionRange, string label)
	{
		GameObject obj = Object.Instantiate(textPrefab);
		obj.transform.SetParent(actionGroupTransform);
		obj.transform.SetAsLastSibling();
		obj.GetComponent<Text>().text = label;
		GameObject gameObject = Object.Instantiate(buttonPrefab);
		gameObject.transform.SetParent(fieldGroupTransform);
		gameObject.transform.SetAsLastSibling();
		rows.Add(new Row
		{
			action = action,
			actionRange = actionRange,
			button = gameObject.GetComponent<Button>(),
			text = gameObject.GetComponentInChildren<Text>()
		});
	}

	private void OnInputFieldClicked(int index, int actionElementMapToReplaceId)
	{
		if (index >= 0 && index < rows.Count)
		{
			ControllerMap map = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, "Default", "Default");
			ControllerMap map2 = player.controllers.maps.GetMap(ControllerType.Mouse, 0, "Default", "Default");
			ControllerMap controllerMap = (map.ContainsElementMap(actionElementMapToReplaceId) ? map : ((!map2.ContainsElementMap(actionElementMapToReplaceId)) ? null : map2));
			_replaceTargetMapping = new TargetMapping
			{
				actionElementMapId = actionElementMapToReplaceId,
				controllerMap = controllerMap
			};
			StartCoroutine(StartListeningDelayed(index, map, map2, actionElementMapToReplaceId));
		}
	}

	private IEnumerator StartListeningDelayed(int index, ControllerMap keyboardMap, ControllerMap mouseMap, int actionElementMapToReplaceId)
	{
		yield return new WaitForSeconds(0.1f);
		inputMapper_keyboard.Start(new InputMapper.Context
		{
			actionId = rows[index].action.id,
			controllerMap = keyboardMap,
			actionRange = rows[index].actionRange,
			actionElementMapToReplace = keyboardMap.GetElementMap(actionElementMapToReplaceId)
		});
		inputMapper_mouse.Start(new InputMapper.Context
		{
			actionId = rows[index].action.id,
			controllerMap = mouseMap,
			actionRange = rows[index].actionRange,
			actionElementMapToReplace = mouseMap.GetElementMap(actionElementMapToReplaceId)
		});
		player.controllers.maps.SetMapsEnabled(state: false, "UI");
		statusUIText.text = "Listening...";
	}

	private void OnInputMapped(InputMapper.InputMappedEventData data)
	{
		inputMapper_keyboard.Stop();
		inputMapper_mouse.Stop();
		if (_replaceTargetMapping.controllerMap != null && data.actionElementMap.controllerMap != _replaceTargetMapping.controllerMap)
		{
			_replaceTargetMapping.controllerMap.DeleteElementMap(_replaceTargetMapping.actionElementMapId);
		}
		RedrawUI();
	}

	private void OnStopped(InputMapper.StoppedEventData data)
	{
		statusUIText.text = string.Empty;
		player.controllers.maps.SetMapsEnabled(state: true, "UI");
	}
}
