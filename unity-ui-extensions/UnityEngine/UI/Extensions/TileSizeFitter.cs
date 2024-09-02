using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("Layout/Extensions/Tile Size Fitter")]
public class TileSizeFitter : UIBehaviour, ILayoutSelfController, ILayoutController
{
	[SerializeField]
	private Vector2 m_Border = Vector2.zero;

	[SerializeField]
	private Vector2 m_TileSize = Vector2.zero;

	[NonSerialized]
	private RectTransform m_Rect;

	private DrivenRectTransformTracker m_Tracker;

	public Vector2 Border
	{
		get
		{
			return m_Border;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_Border, value))
			{
				SetDirty();
			}
		}
	}

	public Vector2 TileSize
	{
		get
		{
			return m_TileSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_TileSize, value))
			{
				SetDirty();
			}
		}
	}

	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
			{
				m_Rect = GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		m_Tracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		UpdateRect();
	}

	private void UpdateRect()
	{
		if (IsActive())
		{
			m_Tracker.Clear();
			m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition);
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.anchoredPosition = Vector2.zero;
			m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta);
			Vector2 vector = GetParentSize() - Border;
			if (TileSize.x > 0.001f)
			{
				vector.x -= Mathf.Floor(vector.x / TileSize.x) * TileSize.x;
			}
			else
			{
				vector.x = 0f;
			}
			if (TileSize.y > 0.001f)
			{
				vector.y -= Mathf.Floor(vector.y / TileSize.y) * TileSize.y;
			}
			else
			{
				vector.y = 0f;
			}
			rectTransform.sizeDelta = -vector;
		}
	}

	private Vector2 GetParentSize()
	{
		RectTransform rectTransform = this.rectTransform.parent as RectTransform;
		if (!rectTransform)
		{
			return Vector2.zero;
		}
		return rectTransform.rect.size;
	}

	public virtual void SetLayoutHorizontal()
	{
	}

	public virtual void SetLayoutVertical()
	{
	}

	protected void SetDirty()
	{
		if (IsActive())
		{
			UpdateRect();
		}
	}
}
