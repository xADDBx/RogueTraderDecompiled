using System;
using System.Collections.Generic;
using Kingmaker.QA.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.UI.Pointer;

[AddComponentMenu("Event/Kingmaker Graphic Raycaster")]
[RequireComponent(typeof(Canvas))]
public class KingmakerGraphicRaycaster : BaseRaycaster
{
	public enum BlockingObjects
	{
		None,
		TwoD,
		ThreeD,
		All
	}

	protected const int kNoEventMaskSet = -1;

	[FormerlySerializedAs("ignoreReversedGraphics")]
	[SerializeField]
	private bool m_IgnoreReversedGraphics = true;

	[FormerlySerializedAs("blockingObjects")]
	[SerializeField]
	private BlockingObjects m_BlockingObjects;

	[SerializeField]
	protected LayerMask m_BlockingMask = -1;

	private Canvas m_Canvas;

	[NonSerialized]
	private List<Graphic> m_RaycastResults = new List<Graphic>();

	[NonSerialized]
	private static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();

	public override int sortOrderPriority
	{
		get
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				return canvas.sortingOrder;
			}
			return base.sortOrderPriority;
		}
	}

	public override int renderOrderPriority
	{
		get
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				return canvas.renderOrder;
			}
			return base.renderOrderPriority;
		}
	}

	public bool ignoreReversedGraphics
	{
		get
		{
			return m_IgnoreReversedGraphics;
		}
		set
		{
			m_IgnoreReversedGraphics = value;
		}
	}

	public BlockingObjects blockingObjects
	{
		get
		{
			return m_BlockingObjects;
		}
		set
		{
			m_BlockingObjects = value;
		}
	}

	private Canvas canvas
	{
		get
		{
			if (m_Canvas != null)
			{
				return m_Canvas;
			}
			m_Canvas = GetComponent<Canvas>();
			return m_Canvas;
		}
	}

	public override Camera eventCamera
	{
		get
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null))
			{
				return null;
			}
			if (!(canvas.worldCamera != null))
			{
				return null;
			}
			return canvas.worldCamera;
		}
	}

	protected KingmakerGraphicRaycaster()
	{
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		using (Counters.UIRaycast?.Measure())
		{
			if (canvas == null)
			{
				return;
			}
			Vector2 vector = ((!(eventCamera == null)) ? ((Vector2)eventCamera.ScreenToViewportPoint(eventData.position)) : new Vector2(eventData.position.x / (float)Screen.width, eventData.position.y / (float)Screen.height));
			if (vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f)
			{
				return;
			}
			float num = float.MaxValue;
			Ray ray = default(Ray);
			if (eventCamera != null)
			{
				ray = eventCamera.ScreenPointToRay(eventData.position);
			}
			if (canvas.renderMode != 0 && blockingObjects != 0)
			{
				float num2 = 100f;
				if (eventCamera != null)
				{
					num2 = eventCamera.farClipPlane - eventCamera.nearClipPlane;
				}
				if ((blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All) && Physics.Raycast(ray, out var hitInfo, num2, m_BlockingMask))
				{
					num = hitInfo.distance;
				}
				if (blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All)
				{
					RaycastHit2D raycastHit2D = Physics2D.Raycast(ray.origin, ray.direction, num2, m_BlockingMask);
					if (raycastHit2D.collider != null)
					{
						num = raycastHit2D.fraction * num2;
					}
				}
			}
			m_RaycastResults.Clear();
			Raycast(canvas, eventCamera, eventData.position, m_RaycastResults);
			for (int i = 0; i < m_RaycastResults.Count; i++)
			{
				GameObject gameObject = m_RaycastResults[i].gameObject;
				bool flag = true;
				if (ignoreReversedGraphics)
				{
					if (eventCamera == null)
					{
						Vector3 rhs = gameObject.transform.rotation * Vector3.forward;
						flag = Vector3.Dot(Vector3.forward, rhs) > 0f;
					}
					else
					{
						Vector3 lhs = eventCamera.transform.rotation * Vector3.forward;
						Vector3 rhs2 = gameObject.transform.rotation * Vector3.forward;
						flag = Vector3.Dot(lhs, rhs2) > 0f;
					}
				}
				if (!flag)
				{
					continue;
				}
				float num3 = 0f;
				if (eventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					num3 = 0f;
				}
				else
				{
					Transform transform = gameObject.transform;
					Vector3 forward = transform.forward;
					num3 = Vector3.Dot(forward, transform.position - ray.origin) / Vector3.Dot(forward, ray.direction);
					if (num3 < 0f)
					{
						continue;
					}
				}
				if (!(num3 >= num))
				{
					RaycastResult raycastResult = default(RaycastResult);
					raycastResult.gameObject = gameObject;
					raycastResult.module = this;
					raycastResult.distance = num3;
					raycastResult.screenPosition = eventData.position;
					raycastResult.index = resultAppendList.Count;
					raycastResult.depth = m_RaycastResults[i].depth;
					raycastResult.sortingLayer = canvas.sortingLayerID;
					raycastResult.sortingOrder = canvas.sortingOrder;
					RaycastResult item = raycastResult;
					resultAppendList.Add(item);
				}
			}
		}
	}

	private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, List<Graphic> results)
	{
		IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(canvas);
		for (int i = 0; i < graphicsForCanvas.Count; i++)
		{
			Graphic graphic = graphicsForCanvas[i];
			if (graphic.raycastTarget && RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera) && graphic.Raycast(pointerPosition, eventCamera))
			{
				s_SortedGraphics.Add(graphic);
			}
		}
		s_SortedGraphics.Sort((Graphic g1, Graphic g2) => g2.depth.CompareTo(g1.depth));
		for (int j = 0; j < s_SortedGraphics.Count; j++)
		{
			results.Add(s_SortedGraphics[j]);
		}
		s_SortedGraphics.Clear();
	}
}
