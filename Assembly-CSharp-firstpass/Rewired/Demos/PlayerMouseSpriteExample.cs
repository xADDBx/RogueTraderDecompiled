using System;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class PlayerMouseSpriteExample : MonoBehaviour
{
	[Tooltip("The Player that will control the mouse")]
	public int playerId;

	[Tooltip("The Rewired Action used for the mouse horizontal axis.")]
	public string horizontalAction = "MouseX";

	[Tooltip("The Rewired Action used for the mouse vertical axis.")]
	public string verticalAction = "MouseY";

	[Tooltip("The Rewired Action used for the mouse wheel axis.")]
	public string wheelAction = "MouseWheel";

	[Tooltip("The Rewired Action used for the mouse left button.")]
	public string leftButtonAction = "MouseLeftButton";

	[Tooltip("The Rewired Action used for the mouse right button.")]
	public string rightButtonAction = "MouseRightButton";

	[Tooltip("The Rewired Action used for the mouse middle button.")]
	public string middleButtonAction = "MouseMiddleButton";

	[Tooltip("The distance from the camera that the pointer will be drawn.")]
	public float distanceFromCamera = 1f;

	[Tooltip("The scale of the sprite pointer.")]
	public float spriteScale = 0.05f;

	[Tooltip("The pointer prefab.")]
	public GameObject pointerPrefab;

	[Tooltip("The click effect prefab.")]
	public GameObject clickEffectPrefab;

	[Tooltip("Should the hardware pointer be hidden?")]
	public bool hideHardwarePointer = true;

	[NonSerialized]
	private GameObject pointer;

	[NonSerialized]
	private PlayerMouse mouse;

	private void Awake()
	{
		pointer = UnityEngine.Object.Instantiate(pointerPrefab);
		pointer.transform.localScale = new Vector3(spriteScale, spriteScale, spriteScale);
		if (hideHardwarePointer)
		{
			Cursor.visible = false;
		}
		mouse = PlayerMouse.Factory.Create();
		mouse.playerId = playerId;
		mouse.xAxis.actionName = horizontalAction;
		mouse.yAxis.actionName = verticalAction;
		mouse.wheel.yAxis.actionName = wheelAction;
		mouse.leftButton.actionName = leftButtonAction;
		mouse.rightButton.actionName = rightButtonAction;
		mouse.middleButton.actionName = middleButtonAction;
		mouse.pointerSpeed = 1f;
		mouse.wheel.yAxis.repeatRate = 5f;
		mouse.screenPosition = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
		mouse.ScreenPositionChangedEvent += OnScreenPositionChanged;
		OnScreenPositionChanged(mouse.screenPosition);
	}

	private void Update()
	{
		if (ReInput.isReady)
		{
			pointer.transform.Rotate(Vector3.forward, mouse.wheel.yAxis.value * 20f);
			if (mouse.leftButton.justPressed)
			{
				CreateClickEffect(new Color(0f, 1f, 0f, 1f));
			}
			if (mouse.rightButton.justPressed)
			{
				CreateClickEffect(new Color(1f, 0f, 0f, 1f));
			}
			if (mouse.middleButton.justPressed)
			{
				CreateClickEffect(new Color(1f, 1f, 0f, 1f));
			}
		}
	}

	private void OnDestroy()
	{
		if (ReInput.isReady)
		{
			mouse.ScreenPositionChangedEvent -= OnScreenPositionChanged;
		}
	}

	private void CreateClickEffect(Color color)
	{
		GameObject obj = UnityEngine.Object.Instantiate(clickEffectPrefab);
		obj.transform.localScale = new Vector3(spriteScale, spriteScale, spriteScale);
		obj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mouse.screenPosition.x, mouse.screenPosition.y, distanceFromCamera));
		obj.GetComponentInChildren<SpriteRenderer>().color = color;
		UnityEngine.Object.Destroy(obj, 0.5f);
	}

	private void OnScreenPositionChanged(Vector2 position)
	{
		Vector3 position2 = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, distanceFromCamera));
		pointer.transform.position = position2;
	}
}
