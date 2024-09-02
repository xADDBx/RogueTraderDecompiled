using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(Canvas))]
[AddComponentMenu("UI/Extensions/Selection Box")]
public class SelectionBox : MonoBehaviour
{
	public class SelectionEvent : UnityEvent<IBoxSelectable[]>
	{
	}

	public Color color;

	public Sprite art;

	private Vector2 origin;

	public RectTransform selectionMask;

	private RectTransform boxRect;

	private IBoxSelectable[] selectables;

	private MonoBehaviour[] selectableGroup;

	private IBoxSelectable clickedBeforeDrag;

	private IBoxSelectable clickedAfterDrag;

	public SelectionEvent onSelectionChange = new SelectionEvent();

	private void ValidateCanvas()
	{
		if (base.gameObject.GetComponent<Canvas>().renderMode != 0)
		{
			throw new Exception("SelectionBox component must be placed on a canvas in Screen Space Overlay mode.");
		}
		CanvasScaler component = base.gameObject.GetComponent<CanvasScaler>();
		if ((bool)component && component.enabled && (!Mathf.Approximately(component.scaleFactor, 1f) || component.uiScaleMode != 0))
		{
			Object.Destroy(component);
			Debug.LogWarning("SelectionBox component is on a gameObject with a Canvas Scaler component. As of now, Canvas Scalers without the default settings throw off the coordinates of the selection box. Canvas Scaler has been removed.");
		}
	}

	private void SetSelectableGroup(IEnumerable<MonoBehaviour> behaviourCollection)
	{
		if (behaviourCollection == null)
		{
			selectableGroup = null;
			return;
		}
		List<MonoBehaviour> list = new List<MonoBehaviour>();
		foreach (MonoBehaviour item in behaviourCollection)
		{
			if (item is IBoxSelectable)
			{
				list.Add(item);
			}
		}
		selectableGroup = list.ToArray();
	}

	private void CreateBoxRect()
	{
		GameObject gameObject = new GameObject("Selection Box");
		gameObject.transform.parent = base.transform;
		gameObject.AddComponent<Image>();
		boxRect = gameObject.transform as RectTransform;
	}

	private void ResetBoxRect()
	{
		Image component = boxRect.GetComponent<Image>();
		component.color = color;
		component.sprite = art;
		origin = Vector2.zero;
		boxRect.anchoredPosition = Vector2.zero;
		boxRect.sizeDelta = Vector2.zero;
		boxRect.anchorMax = Vector2.zero;
		boxRect.anchorMin = Vector2.zero;
		boxRect.pivot = Vector2.zero;
		boxRect.gameObject.SetActive(value: false);
	}

	private void BeginSelection()
	{
		if (!Input.GetMouseButtonDown(0))
		{
			return;
		}
		boxRect.gameObject.SetActive(value: true);
		origin = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		if (!PointIsValidAgainstSelectionMask(origin))
		{
			ResetBoxRect();
			return;
		}
		boxRect.anchoredPosition = origin;
		MonoBehaviour[] array = ((selectableGroup != null) ? selectableGroup : Object.FindObjectsOfType<MonoBehaviour>());
		List<IBoxSelectable> list = new List<IBoxSelectable>();
		MonoBehaviour[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] is IBoxSelectable boxSelectable)
			{
				list.Add(boxSelectable);
				if (!Input.GetKey(KeyCode.LeftShift))
				{
					boxSelectable.selected = false;
				}
			}
		}
		selectables = list.ToArray();
		clickedBeforeDrag = GetSelectableAtMousePosition();
	}

	private bool PointIsValidAgainstSelectionMask(Vector2 screenPoint)
	{
		if (!selectionMask)
		{
			return true;
		}
		Camera screenPointCamera = GetScreenPointCamera(selectionMask);
		return RectTransformUtility.RectangleContainsScreenPoint(selectionMask, screenPoint, screenPointCamera);
	}

	private IBoxSelectable GetSelectableAtMousePosition()
	{
		if (!PointIsValidAgainstSelectionMask(Input.mousePosition))
		{
			return null;
		}
		IBoxSelectable[] array = selectables;
		foreach (IBoxSelectable boxSelectable in array)
		{
			RectTransform rectTransform = boxSelectable.transform as RectTransform;
			if ((bool)rectTransform)
			{
				Camera screenPointCamera = GetScreenPointCamera(rectTransform);
				if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, screenPointCamera))
				{
					return boxSelectable;
				}
			}
			else
			{
				float magnitude = boxSelectable.transform.GetComponent<Renderer>().bounds.extents.magnitude;
				if (Vector2.Distance(GetScreenPointOfSelectable(boxSelectable), Input.mousePosition) <= magnitude)
				{
					return boxSelectable;
				}
			}
		}
		return null;
	}

	private void DragSelection()
	{
		if (Input.GetMouseButton(0) && boxRect.gameObject.activeSelf)
		{
			Vector2 vector = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Vector2 sizeDelta = vector - origin;
			Vector2 anchoredPosition = origin;
			if (sizeDelta.x < 0f)
			{
				anchoredPosition.x = vector.x;
				sizeDelta.x = 0f - sizeDelta.x;
			}
			if (sizeDelta.y < 0f)
			{
				anchoredPosition.y = vector.y;
				sizeDelta.y = 0f - sizeDelta.y;
			}
			boxRect.anchoredPosition = anchoredPosition;
			boxRect.sizeDelta = sizeDelta;
			IBoxSelectable[] array = selectables;
			foreach (IBoxSelectable boxSelectable in array)
			{
				Vector3 vector2 = GetScreenPointOfSelectable(boxSelectable);
				boxSelectable.preSelected = RectTransformUtility.RectangleContainsScreenPoint(boxRect, vector2, null) && PointIsValidAgainstSelectionMask(vector2);
			}
			if (clickedBeforeDrag != null)
			{
				clickedBeforeDrag.preSelected = true;
			}
		}
	}

	private void ApplySingleClickDeselection()
	{
		if (clickedBeforeDrag != null && clickedAfterDrag != null && clickedBeforeDrag.selected && clickedBeforeDrag.transform == clickedAfterDrag.transform)
		{
			clickedBeforeDrag.selected = false;
			clickedBeforeDrag.preSelected = false;
		}
	}

	private void ApplyPreSelections()
	{
		IBoxSelectable[] array = selectables;
		foreach (IBoxSelectable boxSelectable in array)
		{
			if (boxSelectable.preSelected)
			{
				boxSelectable.selected = true;
				boxSelectable.preSelected = false;
			}
		}
	}

	private Vector2 GetScreenPointOfSelectable(IBoxSelectable selectable)
	{
		RectTransform rectTransform = selectable.transform as RectTransform;
		if ((bool)rectTransform)
		{
			return RectTransformUtility.WorldToScreenPoint(GetScreenPointCamera(rectTransform), selectable.transform.position);
		}
		return Camera.main.WorldToScreenPoint(selectable.transform.position);
	}

	private Camera GetScreenPointCamera(RectTransform rectTransform)
	{
		Canvas canvas = null;
		RectTransform rectTransform2 = rectTransform;
		do
		{
			canvas = rectTransform2.GetComponent<Canvas>();
			if ((bool)canvas && !canvas.isRootCanvas)
			{
				canvas = null;
			}
			rectTransform2 = (RectTransform)rectTransform2.parent;
		}
		while (canvas == null);
		switch (canvas.renderMode)
		{
		case RenderMode.ScreenSpaceOverlay:
			return null;
		case RenderMode.ScreenSpaceCamera:
			if (!canvas.worldCamera)
			{
				return Camera.main;
			}
			return canvas.worldCamera;
		default:
			return Camera.main;
		}
	}

	public IBoxSelectable[] GetAllSelected()
	{
		if (selectables == null)
		{
			return new IBoxSelectable[0];
		}
		List<IBoxSelectable> list = new List<IBoxSelectable>();
		IBoxSelectable[] array = selectables;
		foreach (IBoxSelectable boxSelectable in array)
		{
			if (boxSelectable.selected)
			{
				list.Add(boxSelectable);
			}
		}
		return list.ToArray();
	}

	private void EndSelection()
	{
		if (Input.GetMouseButtonUp(0) && boxRect.gameObject.activeSelf)
		{
			clickedAfterDrag = GetSelectableAtMousePosition();
			ApplySingleClickDeselection();
			ApplyPreSelections();
			ResetBoxRect();
			onSelectionChange.Invoke(GetAllSelected());
		}
	}

	private void Start()
	{
		ValidateCanvas();
		CreateBoxRect();
		ResetBoxRect();
	}

	private void Update()
	{
		BeginSelection();
		DragSelection();
		EndSelection();
	}
}
