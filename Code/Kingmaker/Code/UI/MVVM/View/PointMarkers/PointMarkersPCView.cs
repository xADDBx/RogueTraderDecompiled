using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.Common;
using Kingmaker.Code.UI.MVVM.VM.PointMarkers;
using Kingmaker.UI;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.PointMarkers;

[RequireComponent(typeof(RectTransform))]
public class PointMarkersPCView : CommonStaticComponentView<PointMarkersVM>
{
	[SerializeField]
	private RectTransform m_MarkersContainer;

	[SerializeField]
	private PointMarkerPCView m_MarkerView;

	[SerializeField]
	private RectTransform[] m_InfluencingContainers;

	private readonly List<PointMarkerPCView> m_Markers = new List<PointMarkerPCView>();

	private CanvasScalerWorkaround m_CanvasScaler;

	private readonly List<LineSegment2> m_Borders = new List<LineSegment2>();

	private RectTransform m_RectTransform;

	private float m_XMin;

	private float m_XMax;

	private float m_YMin;

	private float m_YMax;

	private List<LineSegment2> m_ParsedBorders = new List<LineSegment2>();

	public List<LineSegment2> Borders => m_Borders;

	public RectTransform RectTransform => m_RectTransform;

	private Camera UICamera => Kingmaker.UI.UICamera.Instance;

	public float ScreenScale
	{
		get
		{
			if (!(m_CanvasScaler != null))
			{
				return 1f;
			}
			return Mathf.Min((float)Screen.width / m_CanvasScaler.referenceResolution.x, (float)Screen.height / m_CanvasScaler.referenceResolution.y);
		}
	}

	public void Initialize(CanvasScalerWorkaround canvasScaler)
	{
		m_CanvasScaler = canvasScaler;
		m_RectTransform = GetComponent<RectTransform>();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.PointMarkers.ObserveAdd().Subscribe(delegate
		{
			DrawMarkers();
		}));
		AddDisposable(base.ViewModel.PointMarkers.ObserveRemove().Subscribe(delegate
		{
			DrawMarkers();
		}));
		AddDisposable(base.ViewModel.PointMarkers.ObserveReset().Subscribe(delegate
		{
			ClearMarkers();
		}));
		DrawMarkers();
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			UpdateHandler();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		ClearMarkers();
	}

	private void UpdateHandler()
	{
		if (base.ViewModel.VisibleMarkers.Any())
		{
			UpdateBorders();
		}
	}

	private void DrawMarkers()
	{
		ClearMarkers();
		foreach (PointMarkerVM pointMarker in base.ViewModel.PointMarkers)
		{
			PointMarkerPCView widget = WidgetFactory.GetWidget(m_MarkerView);
			widget.Initialize(this);
			widget.Bind(pointMarker);
			widget.transform.SetParent(m_MarkersContainer.transform, worldPositionStays: false);
			m_Markers.Add(widget);
		}
	}

	private void ClearMarkers()
	{
		m_Markers.ForEach(delegate(PointMarkerPCView marker)
		{
			marker.Bind(null);
			WidgetFactory.DisposeWidget(marker);
		});
		m_Markers.Clear();
	}

	private void UpdateBorders()
	{
		m_Borders.Clear();
		RectTransform[] influencingContainers = m_InfluencingContainers;
		foreach (RectTransform rectTransform in influencingContainers)
		{
			if (rectTransform != null && rectTransform.gameObject.activeInHierarchy)
			{
				m_Borders.AddRange(ParseBorders(rectTransform));
			}
		}
	}

	private List<LineSegment2> ParseBorders(RectTransform container)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, UICamera.WorldToScreenPoint(container.position), UICamera, out var localPoint);
		Vector2 vector = new Vector2((localPoint.x + m_RectTransform.rect.width * m_RectTransform.pivot.x) * ScreenScale, (localPoint.y + m_RectTransform.rect.height * m_RectTransform.pivot.y) * ScreenScale);
		m_XMin = vector.x - container.rect.width * container.localScale.x * ScreenScale * container.pivot.x;
		m_XMax = vector.x - container.rect.width * container.localScale.x * ScreenScale * (container.pivot.x - 1f);
		m_YMin = vector.y - container.rect.height * container.localScale.y * ScreenScale * container.pivot.y;
		m_YMax = vector.y - container.rect.height * container.localScale.y * ScreenScale * (container.pivot.y - 1f);
		m_ParsedBorders.Clear();
		m_ParsedBorders.Add(new LineSegment2
		{
			PointA = new Vector2(m_XMin, m_YMin),
			PointB = new Vector2(m_XMin, m_YMax)
		});
		m_ParsedBorders.Add(new LineSegment2
		{
			PointA = new Vector2(m_XMin, m_YMin),
			PointB = new Vector2(m_XMax, m_YMin)
		});
		m_ParsedBorders.Add(new LineSegment2
		{
			PointA = new Vector2(m_XMax, m_YMin),
			PointB = new Vector2(m_XMax, m_YMax)
		});
		m_ParsedBorders.Add(new LineSegment2
		{
			PointA = new Vector2(m_XMin, m_YMax),
			PointB = new Vector2(m_XMax, m_YMax)
		});
		return m_ParsedBorders;
	}
}
