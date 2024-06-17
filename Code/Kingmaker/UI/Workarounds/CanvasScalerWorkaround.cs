using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Workarounds;

[RequireComponent(typeof(Canvas))]
[ExecuteInEditMode]
[AddComponentMenu("Layout/Canvas Scaler Workaround", 101)]
public class CanvasScalerWorkaround : UIBehaviour
{
	public enum ScaleMode
	{
		ConstantPixelSize,
		ScaleWithScreenSize,
		ConstantPhysicalSize
	}

	public enum ScreenMatchMode
	{
		MatchWidthOrHeight,
		Expand,
		Shrink
	}

	public enum Unit
	{
		Centimeters,
		Millimeters,
		Inches,
		Points,
		Picas
	}

	[Tooltip("Determines how UI elements in the Canvas are scaled.")]
	[SerializeField]
	private ScaleMode m_UiScaleMode;

	[Tooltip("If a sprite has this 'Pixels Per Unit' setting, then one pixel in the sprite will cover one unit in the UI.")]
	[SerializeField]
	protected float m_ReferencePixelsPerUnit = 100f;

	[Tooltip("Scales all UI elements in the Canvas by this factor.")]
	[SerializeField]
	protected float m_ScaleFactor = 1f;

	[Tooltip("The resolution the UI layout is designed for. If the screen resolution is larger, the UI will be scaled up, and if it's smaller, the UI will be scaled down. This is done in accordance with the Screen Match Mode.")]
	[SerializeField]
	protected Vector2 m_ReferenceResolution = new Vector2(800f, 600f);

	[Tooltip("A mode used to scale the canvas area if the aspect ratio of the current resolution doesn't fit the reference resolution.")]
	[SerializeField]
	protected ScreenMatchMode m_ScreenMatchMode;

	[Tooltip("Determines if the scaling is using the width or height as reference, or a mix in between.")]
	[Range(0f, 1f)]
	[SerializeField]
	protected float m_MatchWidthOrHeight;

	[Tooltip("The physical unit to specify positions and sizes in.")]
	[SerializeField]
	protected Unit m_PhysicalUnit = Unit.Points;

	[Tooltip("The DPI to assume if the screen DPI is not known.")]
	[SerializeField]
	protected float m_FallbackScreenDPI = 96f;

	[Tooltip("The pixels per inch to use for sprites that have a 'Pixels Per Unit' setting that matches the 'Reference Pixels Per Unit' setting.")]
	[SerializeField]
	protected float m_DefaultSpriteDPI = 96f;

	[Tooltip("The amount of pixels per unit to use for dynamically created bitmaps in the UI, such as Text.")]
	[SerializeField]
	protected float m_DynamicPixelsPerUnit = 1f;

	[NonSerialized]
	private float m_PrevScaleFactor = 1f;

	[NonSerialized]
	private float m_PrevReferencePixelsPerUnit = 100f;

	private Canvas m_Canvas;

	public ScaleMode uiScaleMode
	{
		get
		{
			return m_UiScaleMode;
		}
		set
		{
			m_UiScaleMode = value;
		}
	}

	public float referencePixelsPerUnit
	{
		get
		{
			return m_ReferencePixelsPerUnit;
		}
		set
		{
			m_ReferencePixelsPerUnit = value;
		}
	}

	public float scaleFactor
	{
		get
		{
			return m_ScaleFactor;
		}
		set
		{
			m_ScaleFactor = Mathf.Max(0.01f, value);
		}
	}

	public Vector2 referenceResolution
	{
		get
		{
			return m_ReferenceResolution;
		}
		set
		{
			m_ReferenceResolution = value;
			if ((double)m_ReferenceResolution.x > -9.99999974737875E-06 && (double)m_ReferenceResolution.x < 9.99999974737875E-06)
			{
				m_ReferenceResolution.x = 1E-05f * Mathf.Sign(m_ReferenceResolution.x);
			}
			if (!((double)m_ReferenceResolution.y <= -9.99999974737875E-06) && !((double)m_ReferenceResolution.y >= 9.99999974737875E-06))
			{
				m_ReferenceResolution.y = 1E-05f * Mathf.Sign(m_ReferenceResolution.y);
			}
		}
	}

	public ScreenMatchMode screenMatchMode
	{
		get
		{
			return m_ScreenMatchMode;
		}
		set
		{
			m_ScreenMatchMode = value;
		}
	}

	public float matchWidthOrHeight
	{
		get
		{
			return m_MatchWidthOrHeight;
		}
		set
		{
			m_MatchWidthOrHeight = value;
		}
	}

	public Unit physicalUnit
	{
		get
		{
			return m_PhysicalUnit;
		}
		set
		{
			m_PhysicalUnit = value;
		}
	}

	public float fallbackScreenDPI
	{
		get
		{
			return m_FallbackScreenDPI;
		}
		set
		{
			m_FallbackScreenDPI = value;
		}
	}

	public float defaultSpriteDPI
	{
		get
		{
			return m_DefaultSpriteDPI;
		}
		set
		{
			m_DefaultSpriteDPI = Mathf.Max(1f, value);
		}
	}

	public float dynamicPixelsPerUnit
	{
		get
		{
			return m_DynamicPixelsPerUnit;
		}
		set
		{
			m_DynamicPixelsPerUnit = value;
		}
	}

	protected CanvasScalerWorkaround()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_Canvas = GetComponent<Canvas>();
		Handle();
	}

	protected override void OnDisable()
	{
		SetScaleFactor(1f);
		SetReferencePixelsPerUnit(100f);
		base.OnDisable();
	}

	protected virtual void Update()
	{
		Handle();
	}

	protected virtual void Handle()
	{
		if (m_Canvas == null || !m_Canvas.isRootCanvas)
		{
			return;
		}
		if (m_Canvas.renderMode == RenderMode.WorldSpace)
		{
			HandleWorldCanvas();
			return;
		}
		switch (m_UiScaleMode)
		{
		case ScaleMode.ConstantPixelSize:
			HandleConstantPixelSize();
			break;
		case ScaleMode.ScaleWithScreenSize:
			HandleScaleWithScreenSize();
			break;
		case ScaleMode.ConstantPhysicalSize:
			HandleConstantPhysicalSize();
			break;
		}
	}

	protected virtual void HandleWorldCanvas()
	{
		SetScaleFactor(m_DynamicPixelsPerUnit);
		SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
	}

	protected virtual void HandleConstantPixelSize()
	{
		SetScaleFactor(m_ScaleFactor);
		SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
	}

	protected virtual void HandleScaleWithScreenSize()
	{
		Vector2 vector = new Vector2(Screen.width, Screen.height);
		int targetDisplay = m_Canvas.targetDisplay;
		if (targetDisplay > 0 && targetDisplay < Display.displays.Length)
		{
			Display display = Display.displays[targetDisplay];
			vector = new Vector2(display.renderingWidth, display.renderingHeight);
		}
		float num = 0f;
		switch (m_ScreenMatchMode)
		{
		case ScreenMatchMode.MatchWidthOrHeight:
			num = Mathf.Pow(2f, Mathf.Lerp(Mathf.Log(vector.x / m_ReferenceResolution.x, 2f), Mathf.Log(vector.y / m_ReferenceResolution.y, 2f), m_MatchWidthOrHeight));
			break;
		case ScreenMatchMode.Expand:
			num = Mathf.Min(vector.x / m_ReferenceResolution.x, vector.y / m_ReferenceResolution.y);
			break;
		case ScreenMatchMode.Shrink:
			num = Mathf.Max(vector.x / m_ReferenceResolution.x, vector.y / m_ReferenceResolution.y);
			break;
		}
		SetScaleFactor(num);
		SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
	}

	protected virtual void HandleConstantPhysicalSize()
	{
		float dpi = Screen.dpi;
		float num = (((double)dpi != 0.0) ? dpi : m_FallbackScreenDPI);
		float num2 = 1f;
		switch (m_PhysicalUnit)
		{
		case Unit.Centimeters:
			num2 = 2.54f;
			break;
		case Unit.Millimeters:
			num2 = 25.4f;
			break;
		case Unit.Inches:
			num2 = 1f;
			break;
		case Unit.Points:
			num2 = 72f;
			break;
		case Unit.Picas:
			num2 = 6f;
			break;
		}
		SetScaleFactor(num / num2);
		SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * num2 / m_DefaultSpriteDPI);
	}

	protected void SetScaleFactor(float scaleFactor)
	{
		if (scaleFactor != m_PrevScaleFactor)
		{
			ApplyWorkaround();
			m_Canvas.scaleFactor = scaleFactor;
			m_PrevScaleFactor = scaleFactor;
		}
	}

	protected void SetReferencePixelsPerUnit(float referencePixelsPerUnit)
	{
		if (referencePixelsPerUnit != m_PrevReferencePixelsPerUnit)
		{
			ApplyWorkaround();
			m_Canvas.referencePixelsPerUnit = referencePixelsPerUnit;
			m_PrevReferencePixelsPerUnit = referencePixelsPerUnit;
		}
	}

	private void ApplyWorkaround()
	{
		int childCount = base.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			RectTransform rectTransform = base.transform.GetChild(i) as RectTransform;
			if (rectTransform != null)
			{
				_ = rectTransform.anchoredPosition;
				continue;
			}
			PFLog.Default.Warning("Can not cast {0} to RectTransform", base.transform.name);
		}
	}
}
