using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("Event/VR Input Module")]
public class VRInputModule : BaseInputModule
{
	public static GameObject targetObject;

	private static VRInputModule _singleton;

	private int counter;

	private static bool mouseClicked;

	public static Vector3 cursorPosition;

	protected override void Awake()
	{
		_singleton = this;
	}

	public override void Process()
	{
		if (targetObject == null)
		{
			mouseClicked = false;
		}
	}

	public static void PointerSubmit(GameObject obj)
	{
		targetObject = obj;
		mouseClicked = true;
		if (mouseClicked)
		{
			BaseEventData baseEventData = new BaseEventData(_singleton.eventSystem);
			baseEventData.selectedObject = targetObject;
			ExecuteEvents.Execute(targetObject, baseEventData, ExecuteEvents.submitHandler);
			MonoBehaviour.print("clicked " + targetObject.name);
			mouseClicked = false;
		}
	}

	public static void PointerExit(GameObject obj)
	{
		MonoBehaviour.print("PointerExit " + obj.name);
		PointerEventData eventData = new PointerEventData(_singleton.eventSystem);
		ExecuteEvents.Execute(obj, eventData, ExecuteEvents.pointerExitHandler);
		ExecuteEvents.Execute(obj, eventData, ExecuteEvents.deselectHandler);
	}

	public static void PointerEnter(GameObject obj)
	{
		MonoBehaviour.print("PointerEnter " + obj.name);
		PointerEventData pointerEventData = new PointerEventData(_singleton.eventSystem);
		pointerEventData.pointerEnter = obj;
		RaycastResult raycastResult = default(RaycastResult);
		raycastResult.worldPosition = cursorPosition;
		RaycastResult pointerCurrentRaycast = raycastResult;
		pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
		ExecuteEvents.Execute(obj, pointerEventData, ExecuteEvents.pointerEnterHandler);
	}
}
