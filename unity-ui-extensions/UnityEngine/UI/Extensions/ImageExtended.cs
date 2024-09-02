using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.Sprites;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Image Extended")]
public class ImageExtended : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
{
	public enum Type
	{
		Simple,
		Sliced,
		Tiled,
		Filled
	}

	public enum FillMethod
	{
		Horizontal,
		Vertical,
		Radial90,
		Radial180,
		Radial360
	}

	public enum OriginHorizontal
	{
		Left,
		Right
	}

	public enum OriginVertical
	{
		Bottom,
		Top
	}

	public enum Origin90
	{
		BottomLeft,
		TopLeft,
		TopRight,
		BottomRight
	}

	public enum Origin180
	{
		Bottom,
		Left,
		Top,
		Right
	}

	public enum Origin360
	{
		Bottom,
		Right,
		Top,
		Left
	}

	public enum Rotate
	{
		Rotate0,
		Rotate90,
		Rotate180,
		Rotate270
	}

	[FormerlySerializedAs("m_Frame")]
	[SerializeField]
	private Sprite m_Sprite;

	[NonSerialized]
	private Sprite m_OverrideSprite;

	[SerializeField]
	private Type m_Type;

	[SerializeField]
	private bool m_PreserveAspect;

	[SerializeField]
	private bool m_FillCenter = true;

	[SerializeField]
	private FillMethod m_FillMethod = FillMethod.Radial360;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_FillAmount = 1f;

	[SerializeField]
	private bool m_FillClockwise = true;

	[SerializeField]
	private int m_FillOrigin;

	[SerializeField]
	private Rotate m_Rotate;

	private float m_EventAlphaThreshold = 1f;

	private static readonly Vector2[] s_VertScratch = new Vector2[4];

	private static readonly Vector2[] s_UVScratch = new Vector2[4];

	private static readonly Vector2[] s_UVTiled = new Vector2[2];

	private static readonly Vector3[] s_VertQuad = new Vector3[4];

	private static readonly Vector2[] s_UVQuad = new Vector2[4];

	private static readonly Vector2[] s_Xy = new Vector2[4];

	private static readonly Vector2[] s_Uv = new Vector2[4];

	public Sprite sprite
	{
		get
		{
			return m_Sprite;
		}
		set
		{
			if (SetPropertyUtility.SetClass(ref m_Sprite, value))
			{
				SetAllDirty();
			}
		}
	}

	public Sprite overrideSprite
	{
		get
		{
			if (!(m_OverrideSprite == null))
			{
				return m_OverrideSprite;
			}
			return sprite;
		}
		set
		{
			if (SetPropertyUtility.SetClass(ref m_OverrideSprite, value))
			{
				SetAllDirty();
			}
		}
	}

	public Type type
	{
		get
		{
			return m_Type;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_Type, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public bool preserveAspect
	{
		get
		{
			return m_PreserveAspect;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_PreserveAspect, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public bool fillCenter
	{
		get
		{
			return m_FillCenter;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillCenter, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public FillMethod fillMethod
	{
		get
		{
			return m_FillMethod;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillMethod, value))
			{
				SetVerticesDirty();
				m_FillOrigin = 0;
			}
		}
	}

	public float fillAmount
	{
		get
		{
			return m_FillAmount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillAmount, Mathf.Clamp01(value)))
			{
				SetVerticesDirty();
			}
		}
	}

	public bool fillClockwise
	{
		get
		{
			return m_FillClockwise;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillClockwise, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public int fillOrigin
	{
		get
		{
			return m_FillOrigin;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillOrigin, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public Rotate rotate
	{
		get
		{
			return m_Rotate;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_Rotate, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public float eventAlphaThreshold
	{
		get
		{
			return m_EventAlphaThreshold;
		}
		set
		{
			m_EventAlphaThreshold = value;
		}
	}

	public override Texture mainTexture
	{
		get
		{
			if (!(overrideSprite == null))
			{
				return overrideSprite.texture;
			}
			return Graphic.s_WhiteTexture;
		}
	}

	public bool hasBorder
	{
		get
		{
			if (overrideSprite != null)
			{
				return overrideSprite.border.sqrMagnitude > 0f;
			}
			return false;
		}
	}

	public float pixelsPerUnit
	{
		get
		{
			float num = 100f;
			if ((bool)sprite)
			{
				num = sprite.pixelsPerUnit;
			}
			float num2 = 100f;
			if ((bool)base.canvas)
			{
				num2 = base.canvas.referencePixelsPerUnit;
			}
			return num / num2;
		}
	}

	public virtual float minWidth => 0f;

	public virtual float preferredWidth
	{
		get
		{
			if (overrideSprite == null)
			{
				return 0f;
			}
			if (type == Type.Sliced || type == Type.Tiled)
			{
				return DataUtility.GetMinSize(overrideSprite).x / pixelsPerUnit;
			}
			return overrideSprite.rect.size.x / pixelsPerUnit;
		}
	}

	public virtual float flexibleWidth => -1f;

	public virtual float minHeight => 0f;

	public virtual float preferredHeight
	{
		get
		{
			if (overrideSprite == null)
			{
				return 0f;
			}
			if (type == Type.Sliced || type == Type.Tiled)
			{
				return DataUtility.GetMinSize(overrideSprite).y / pixelsPerUnit;
			}
			return overrideSprite.rect.size.y / pixelsPerUnit;
		}
	}

	public virtual float flexibleHeight => -1f;

	public virtual int layoutPriority => 0;

	protected ImageExtended()
	{
	}

	public virtual void OnBeforeSerialize()
	{
	}

	public virtual void OnAfterDeserialize()
	{
		if (m_FillOrigin < 0)
		{
			m_FillOrigin = 0;
		}
		else if (m_FillMethod == FillMethod.Horizontal && m_FillOrigin > 1)
		{
			m_FillOrigin = 0;
		}
		else if (m_FillMethod == FillMethod.Vertical && m_FillOrigin > 1)
		{
			m_FillOrigin = 0;
		}
		else if (m_FillOrigin > 3)
		{
			m_FillOrigin = 0;
		}
		m_FillAmount = Mathf.Clamp(m_FillAmount, 0f, 1f);
	}

	private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
	{
		Vector4 vector = ((overrideSprite == null) ? Vector4.zero : DataUtility.GetPadding(overrideSprite));
		Vector2 vector2 = ((overrideSprite == null) ? Vector2.zero : new Vector2(overrideSprite.rect.width, overrideSprite.rect.height));
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		int num = Mathf.RoundToInt(vector2.x);
		int num2 = Mathf.RoundToInt(vector2.y);
		Vector4 vector3 = ((overrideSprite == null) ? new Vector4(0f, 0f, 1f, 1f) : new Vector4(vector.x / (float)num, vector.y / (float)num2, ((float)num - vector.z) / (float)num, ((float)num2 - vector.w) / (float)num2));
		if (shouldPreserveAspect && vector2.sqrMagnitude > 0f)
		{
			float num3 = vector2.x / vector2.y;
			float num4 = pixelAdjustedRect.width / pixelAdjustedRect.height;
			if (num3 > num4)
			{
				float height = pixelAdjustedRect.height;
				pixelAdjustedRect.height = pixelAdjustedRect.width * (1f / num3);
				pixelAdjustedRect.y += (height - pixelAdjustedRect.height) * base.rectTransform.pivot.y;
			}
			else
			{
				float width = pixelAdjustedRect.width;
				pixelAdjustedRect.width = pixelAdjustedRect.height * num3;
				pixelAdjustedRect.x += (width - pixelAdjustedRect.width) * base.rectTransform.pivot.x;
			}
		}
		return new Vector4(pixelAdjustedRect.x + pixelAdjustedRect.width * vector3.x, pixelAdjustedRect.y + pixelAdjustedRect.height * vector3.y, pixelAdjustedRect.x + pixelAdjustedRect.width * vector3.z, pixelAdjustedRect.y + pixelAdjustedRect.height * vector3.w);
	}

	public override void SetNativeSize()
	{
		if (overrideSprite != null)
		{
			float x = overrideSprite.rect.width / pixelsPerUnit;
			float y = overrideSprite.rect.height / pixelsPerUnit;
			base.rectTransform.anchorMax = base.rectTransform.anchorMin;
			base.rectTransform.sizeDelta = new Vector2(x, y);
			SetAllDirty();
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		List<UIVertex> list = new List<UIVertex>();
		vh.GetUIVertexStream(list);
		switch (type)
		{
		case Type.Simple:
			GenerateSimpleSprite(list, m_PreserveAspect);
			break;
		case Type.Sliced:
			GenerateSlicedSprite(list);
			break;
		case Type.Tiled:
			GenerateTiledSprite(list);
			break;
		case Type.Filled:
			GenerateFilledSprite(list, m_PreserveAspect);
			break;
		}
		vh.Clear();
		vh.AddUIVertexTriangleStream(list);
	}

	private void GenerateSimpleSprite(List<UIVertex> vbo, bool preserveAspect)
	{
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);
		Vector4 vector = ((overrideSprite != null) ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero);
		AddQuad(vbo, simpleVert, new Vector2(drawingDimensions.x, drawingDimensions.y), new Vector2(drawingDimensions.z, drawingDimensions.w), new Vector2(vector.x, vector.y), new Vector2(vector.z, vector.w));
	}

	private void GenerateSlicedSprite(List<UIVertex> vbo)
	{
		if (!hasBorder)
		{
			GenerateSimpleSprite(vbo, preserveAspect: false);
			return;
		}
		Vector4 vector;
		Vector4 vector2;
		Vector4 vector3;
		Vector4 vector4;
		if (overrideSprite != null)
		{
			vector = DataUtility.GetOuterUV(overrideSprite);
			vector2 = DataUtility.GetInnerUV(overrideSprite);
			vector3 = DataUtility.GetPadding(overrideSprite);
			vector4 = overrideSprite.border;
		}
		else
		{
			vector = Vector4.zero;
			vector2 = Vector4.zero;
			vector3 = Vector4.zero;
			vector4 = Vector4.zero;
		}
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		vector4 = GetAdjustedBorders(vector4 / pixelsPerUnit, pixelAdjustedRect);
		vector3 /= pixelsPerUnit;
		int num = (int)(4 - rotate);
		for (int i = 0; i < 4; i++)
		{
			s_VertScratch[(4 - i / 2) % 4][i % 2] = vector3[(i + num) % 4];
			s_VertScratch[1 + i / 2][i % 2] = vector4[(i + num) % 4];
		}
		for (int j = 2; j < 4; j++)
		{
			s_VertScratch[j].x = pixelAdjustedRect.width - s_VertScratch[j].x;
			s_VertScratch[j].y = pixelAdjustedRect.height - s_VertScratch[j].y;
		}
		for (int k = 0; k < 4; k++)
		{
			s_VertScratch[k].x += pixelAdjustedRect.x;
			s_VertScratch[k].y += pixelAdjustedRect.y;
		}
		s_UVScratch[0] = new Vector2(vector.x, vector.y);
		s_UVScratch[1] = new Vector2(vector2.x, vector2.y);
		s_UVScratch[2] = new Vector2(vector2.z, vector2.w);
		s_UVScratch[3] = new Vector2(vector.z, vector.w);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		for (int l = 0; l < 3; l++)
		{
			int num2 = l + 1;
			for (int m = 0; m < 3; m++)
			{
				if (m_FillCenter || l != 1 || m != 1)
				{
					int num3 = m + 1;
					int num4 = l;
					int num5 = m;
					int num6 = num2;
					int num7 = num3;
					for (int n = 0; n < (int)rotate; n++)
					{
						int num8 = 4 - num5 - 1;
						num5 = num4;
						num4 = num8;
						int num9 = 4 - num7 - 1;
						num7 = num6;
						num6 = num9;
					}
					int num10 = l;
					int num11 = m;
					int num12 = num2;
					int num13 = num3;
					if (rotate >= Rotate.Rotate180)
					{
						num10 = num2;
						num12 = l;
					}
					if ((int)(rotate + 1) % 4 >= 2)
					{
						num11 = num3;
						num13 = m;
					}
					if (!(Mathf.Abs(s_VertScratch[num4].x - s_VertScratch[num6].x) < Mathf.Epsilon) && !(Mathf.Abs(s_VertScratch[num5].y - s_VertScratch[num7].y) < Mathf.Epsilon))
					{
						AddQuad(vbo, simpleVert, new Vector2(s_VertScratch[num4].x, s_VertScratch[num5].y), new Vector2(s_VertScratch[num6].x, s_VertScratch[num7].y), new Vector2(s_UVScratch[num10].x, s_UVScratch[num11].y), new Vector2(s_UVScratch[num12].x, s_UVScratch[num13].y));
					}
				}
			}
		}
	}

	private void GenerateTiledSprite(List<UIVertex> vbo)
	{
		Vector4 vector;
		Vector4 vector2;
		Vector2 vector4;
		Vector4 vector3;
		if (overrideSprite != null)
		{
			vector = DataUtility.GetOuterUV(overrideSprite);
			vector2 = DataUtility.GetInnerUV(overrideSprite);
			vector3 = overrideSprite.border;
			vector4 = overrideSprite.rect.size;
		}
		else
		{
			vector = Vector4.zero;
			vector2 = Vector4.zero;
			vector3 = Vector4.zero;
			vector4 = Vector2.one * 100f;
		}
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		float num = (vector4.x - vector3.x - vector3.z) / pixelsPerUnit;
		float num2 = (vector4.y - vector3.y - vector3.w) / pixelsPerUnit;
		vector3 = GetAdjustedBorders(vector3 / pixelsPerUnit, pixelAdjustedRect);
		int num3 = (int)(4 - rotate);
		int index = num3 % 4;
		int index2 = (1 + num3) % 4;
		int index3 = (2 + num3) % 4;
		int index4 = (3 + num3) % 4;
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		float num4 = vector3[index];
		float num5 = pixelAdjustedRect.width - vector3[index3];
		float num6 = vector3[index2];
		float num7 = pixelAdjustedRect.height - vector3[index4];
		if (num5 - num4 > num * 100f || num7 - num6 > num2 * 100f)
		{
			num = (num5 - num4) / 100f;
			num2 = (num7 - num6) / 100f;
		}
		if ((int)rotate % 2 == 1)
		{
			float num8 = num;
			num = num2;
			num2 = num8;
		}
		if (m_FillCenter)
		{
			for (float num9 = num6; num9 < num7; num9 += num2)
			{
				s_UVTiled[0] = new Vector2(vector2.x, vector2.y);
				s_UVTiled[1] = new Vector2(vector2.z, vector2.w);
				float num10 = num9 + num2;
				if (num10 > num7)
				{
					int num11 = 1 - (int)rotate / 2;
					int index5 = 1 - (int)rotate % 2;
					s_UVTiled[num11][index5] = s_UVTiled[1 - num11][index5] + (s_UVTiled[num11][index5] - s_UVTiled[1 - num11][index5]) * (num7 - num9) / (num10 - num9);
					num10 = num7;
				}
				for (float num12 = num4; num12 < num5; num12 += num)
				{
					float num13 = num12 + num;
					if (num13 > num5)
					{
						int num14 = (int)(rotate + 3) % 4 / 2;
						int index6 = (int)rotate % 2;
						s_UVTiled[num14][index6] = s_UVTiled[1 - num14][index6] + (s_UVTiled[num14][index6] - s_UVTiled[1 - num14][index6]) * (num5 - num12) / (num13 - num12);
						num13 = num5;
					}
					AddQuad(vbo, simpleVert, new Vector2(num12, num9) + pixelAdjustedRect.position, new Vector2(num13, num10) + pixelAdjustedRect.position, s_UVTiled[0], s_UVTiled[1]);
				}
			}
		}
		if (!hasBorder)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			float num15 = ((i == 0) ? 0f : num7);
			float num16 = ((i == 0) ? num6 : pixelAdjustedRect.height);
			if (Mathf.Abs(num15 - num16) < Mathf.Epsilon)
			{
				continue;
			}
			s_UVTiled[0] = GetRotatedUV(vector2, 0, (i == 0) ? vector : vector2, (i == 0) ? 1 : 3);
			s_UVTiled[1] = GetRotatedUV(vector2, 2, (i == 0) ? vector2 : vector, (i == 0) ? 1 : 3);
			RotatePairUV(s_UVTiled);
			for (float num17 = num4; num17 < num5; num17 += num)
			{
				float num18 = num17 + num;
				if (num18 > num5)
				{
					int num19 = (int)(rotate + 3) % 4 / 2;
					int index7 = (int)rotate % 2;
					s_UVTiled[num19][index7] = s_UVTiled[1 - num19][index7] + (s_UVTiled[num19][index7] - s_UVTiled[1 - num19][index7]) * (num5 - num17) / (num18 - num17);
					num18 = num5;
				}
				AddQuad(vbo, simpleVert, new Vector2(num17, num15) + pixelAdjustedRect.position, new Vector2(num18, num16) + pixelAdjustedRect.position, s_UVTiled[0], s_UVTiled[1]);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			float num20 = ((j == 0) ? 0f : num5);
			float num21 = ((j == 0) ? num4 : pixelAdjustedRect.width);
			if (Mathf.Abs(num20 - num21) < Mathf.Epsilon)
			{
				continue;
			}
			s_UVTiled[0] = GetRotatedUV((j == 0) ? vector : vector2, (j != 0) ? 2 : 0, vector2, 1);
			s_UVTiled[1] = GetRotatedUV((j == 0) ? vector2 : vector, (j != 0) ? 2 : 0, vector2, 3);
			RotatePairUV(s_UVTiled);
			for (float num22 = num6; num22 < num7; num22 += num2)
			{
				float num23 = num22 + num2;
				if (num23 > num7)
				{
					int num24 = 1 - (int)rotate / 2;
					int index8 = 1 - (int)rotate % 2;
					s_UVTiled[num24][index8] = s_UVTiled[1 - num24][index8] + (s_UVTiled[num24][index8] - s_UVTiled[1 - num24][index8]) * (num7 - num22) / (num23 - num22);
					num23 = num7;
				}
				AddQuad(vbo, simpleVert, new Vector2(num20, num22) + pixelAdjustedRect.position, new Vector2(num21, num23) + pixelAdjustedRect.position, s_UVTiled[0], s_UVTiled[1]);
			}
		}
		if (Mathf.Abs(vector3[index]) > Mathf.Epsilon && Mathf.Abs(vector3[index2]) > Mathf.Epsilon)
		{
			s_UVTiled[0] = GetRotatedUV(vector, 0, vector, 1);
			s_UVTiled[1] = GetRotatedUV(vector2, 0, vector2, 1);
			RotatePairUV(s_UVTiled);
			AddQuad(vbo, simpleVert, new Vector2(0f, 0f) + pixelAdjustedRect.position, new Vector2(num4, num6) + pixelAdjustedRect.position, s_UVTiled[0], s_UVTiled[1]);
		}
		if (Mathf.Abs(vector3[index3]) > Mathf.Epsilon && Mathf.Abs(vector3[index2]) > Mathf.Epsilon)
		{
			s_UVTiled[0] = GetRotatedUV(vector2, 2, vector, 1);
			s_UVTiled[1] = GetRotatedUV(vector, 2, vector2, 1);
			RotatePairUV(s_UVTiled);
			AddQuad(vbo, simpleVert, new Vector2(num5, 0f) + pixelAdjustedRect.position, new Vector2(pixelAdjustedRect.width, num6) + pixelAdjustedRect.position, s_UVTiled[0], s_UVTiled[1]);
		}
		if (Mathf.Abs(vector3[index]) > Mathf.Epsilon && Mathf.Abs(vector3[index4]) > Mathf.Epsilon)
		{
			s_UVTiled[0] = GetRotatedUV(vector, 0, vector2, 3);
			s_UVTiled[1] = GetRotatedUV(vector2, 0, vector, 3);
			RotatePairUV(s_UVTiled);
			AddQuad(vbo, simpleVert, new Vector2(0f, num7) + pixelAdjustedRect.position, new Vector2(num4, pixelAdjustedRect.height) + pixelAdjustedRect.position, s_UVTiled[0], s_UVTiled[1]);
		}
		if (Mathf.Abs(vector3[index3]) > Mathf.Epsilon && Mathf.Abs(vector3[index4]) > Mathf.Epsilon)
		{
			s_UVTiled[0] = GetRotatedUV(vector2, 2, vector2, 3);
			s_UVTiled[1] = GetRotatedUV(vector, 2, vector, 3);
			RotatePairUV(s_UVTiled);
			AddQuad(vbo, simpleVert, new Vector2(num5, num7) + pixelAdjustedRect.position, new Vector2(pixelAdjustedRect.width, pixelAdjustedRect.height) + pixelAdjustedRect.position, s_UVTiled[0], s_UVTiled[1]);
		}
	}

	private Vector2 GetRotatedUV(Vector4 sX, int iX, Vector4 sY, int iY)
	{
		for (int i = 0; i < (int)rotate; i++)
		{
			Vector4 vector = sX;
			sX = sY;
			sY = vector;
			int num = (iX + 3) % 4;
			iX = iY - 1;
			iY = num;
		}
		return new Vector2(sX[iX], sY[iY]);
	}

	private void RotatePairUV(Vector2[] uv)
	{
		if ((int)rotate / 2 == 1)
		{
			float x = uv[0].x;
			uv[0].x = uv[1].x;
			uv[1].x = x;
		}
		if ((int)(rotate + 1) / 2 == 1)
		{
			float y = uv[0].y;
			uv[0].y = uv[1].y;
			uv[1].y = y;
		}
	}

	private void AddQuad(List<UIVertex> vbo, UIVertex v, Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax)
	{
		s_VertQuad[0] = new Vector3(posMin.x, posMin.y, 0f);
		s_VertQuad[1] = new Vector3(posMin.x, posMax.y, 0f);
		s_VertQuad[2] = new Vector3(posMax.x, posMax.y, 0f);
		s_VertQuad[3] = new Vector3(posMax.x, posMin.y, 0f);
		s_UVQuad[0] = new Vector2(uvMin.x, uvMin.y);
		s_UVQuad[1] = new Vector2(uvMin.x, uvMax.y);
		s_UVQuad[2] = new Vector2(uvMax.x, uvMax.y);
		s_UVQuad[3] = new Vector2(uvMax.x, uvMin.y);
		int num = (int)rotate;
		for (int i = 0; i < 4; i++)
		{
			v.position = s_VertQuad[i];
			v.uv0 = s_UVQuad[(i + num) % 4];
			vbo.Add(v);
		}
	}

	private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
	{
		for (int i = 0; i <= 1; i++)
		{
			float num = border[i] + border[i + 2];
			float num2 = rect.size[(i + (int)rotate % 2) % 2];
			if (num2 < num && num != 0f)
			{
				float num3 = num2 / num;
				border[i] *= num3;
				border[i + 2] *= num3;
			}
		}
		return border;
	}

	private void GenerateFilledSprite(List<UIVertex> vbo, bool preserveAspect)
	{
		if (m_FillAmount < 0.001f)
		{
			return;
		}
		Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);
		Vector4 vector = ((overrideSprite != null) ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		int num = (int)(4 - rotate);
		int index = num % 4;
		int index2 = (1 + num) % 4;
		int index3 = (2 + num) % 4;
		int index4 = (3 + num) % 4;
		if (m_FillMethod == FillMethod.Horizontal || m_FillMethod == FillMethod.Vertical)
		{
			if (fillMethod == FillMethod.Horizontal)
			{
				float num2 = (vector[index3] - vector[index]) * m_FillAmount;
				if (m_FillOrigin == 1)
				{
					drawingDimensions.x = drawingDimensions.z - (drawingDimensions.z - drawingDimensions.x) * m_FillAmount;
					vector[index] = vector[index3] - num2;
				}
				else
				{
					drawingDimensions.z = drawingDimensions.x + (drawingDimensions.z - drawingDimensions.x) * m_FillAmount;
					vector[index3] = vector[index] + num2;
				}
			}
			else if (fillMethod == FillMethod.Vertical)
			{
				float num3 = (vector[index4] - vector[index2]) * m_FillAmount;
				if (m_FillOrigin == 1)
				{
					drawingDimensions.y = drawingDimensions.w - (drawingDimensions.w - drawingDimensions.y) * m_FillAmount;
					vector[index2] = vector[index4] - num3;
				}
				else
				{
					drawingDimensions.w = drawingDimensions.y + (drawingDimensions.w - drawingDimensions.y) * m_FillAmount;
					vector[index4] = vector[index2] + num3;
				}
			}
		}
		s_Xy[0] = new Vector2(drawingDimensions.x, drawingDimensions.y);
		s_Xy[1] = new Vector2(drawingDimensions.x, drawingDimensions.w);
		s_Xy[2] = new Vector2(drawingDimensions.z, drawingDimensions.w);
		s_Xy[3] = new Vector2(drawingDimensions.z, drawingDimensions.y);
		s_Uv[num % 4] = new Vector2(vector.x, vector.y);
		s_Uv[(1 + num) % 4] = new Vector2(vector.x, vector.w);
		s_Uv[(2 + num) % 4] = new Vector2(vector.z, vector.w);
		s_Uv[(3 + num) % 4] = new Vector2(vector.z, vector.y);
		if (m_FillAmount < 1f)
		{
			float x = vector.x;
			float y = vector.y;
			float z = vector.z;
			float w = vector.w;
			if (fillMethod == FillMethod.Radial90)
			{
				if (RadialCut(s_Xy, s_Uv, m_FillAmount, m_FillClockwise, m_FillOrigin))
				{
					for (int i = 0; i < 4; i++)
					{
						simpleVert.position = s_Xy[i];
						simpleVert.uv0 = s_Uv[i];
						vbo.Add(simpleVert);
					}
				}
				return;
			}
			if (fillMethod == FillMethod.Radial180)
			{
				for (int j = 0; j < 2; j++)
				{
					int num4 = ((m_FillOrigin > 1) ? 1 : 0);
					float t;
					float t2;
					float t3;
					float t4;
					if (m_FillOrigin == 0 || m_FillOrigin == 2)
					{
						t = 0f;
						t2 = 1f;
						if (j == num4)
						{
							t3 = 0f;
							t4 = 0.5f;
						}
						else
						{
							t3 = 0.5f;
							t4 = 1f;
						}
					}
					else
					{
						t3 = 0f;
						t4 = 1f;
						if (j == num4)
						{
							t = 0.5f;
							t2 = 1f;
						}
						else
						{
							t = 0f;
							t2 = 0.5f;
						}
					}
					s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t3);
					s_Xy[1].x = s_Xy[0].x;
					s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t4);
					s_Xy[3].x = s_Xy[2].x;
					s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t);
					s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t2);
					s_Xy[2].y = s_Xy[1].y;
					s_Xy[3].y = s_Xy[0].y;
					s_Uv[0].x = Mathf.Lerp(x, z, t3);
					s_Uv[1].x = s_Uv[0].x;
					s_Uv[2].x = Mathf.Lerp(x, z, t4);
					s_Uv[3].x = s_Uv[2].x;
					s_Uv[0].y = Mathf.Lerp(y, w, t);
					s_Uv[1].y = Mathf.Lerp(y, w, t2);
					s_Uv[2].y = s_Uv[1].y;
					s_Uv[3].y = s_Uv[0].y;
					float value = (m_FillClockwise ? (fillAmount * 2f - (float)j) : (m_FillAmount * 2f - (float)(1 - j)));
					if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(value), m_FillClockwise, (j + m_FillOrigin + 3) % 4))
					{
						for (int k = 0; k < 4; k++)
						{
							simpleVert.position = s_Xy[k];
							simpleVert.uv0 = s_Uv[k];
							vbo.Add(simpleVert);
						}
					}
				}
				return;
			}
			if (fillMethod == FillMethod.Radial360)
			{
				for (int l = 0; l < 4; l++)
				{
					float t5;
					float t6;
					if (l < 2)
					{
						t5 = 0f;
						t6 = 0.5f;
					}
					else
					{
						t5 = 0.5f;
						t6 = 1f;
					}
					float t7;
					float t8;
					if (l == 0 || l == 3)
					{
						t7 = 0f;
						t8 = 0.5f;
					}
					else
					{
						t7 = 0.5f;
						t8 = 1f;
					}
					s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t5);
					s_Xy[1].x = s_Xy[0].x;
					s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t6);
					s_Xy[3].x = s_Xy[2].x;
					s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t7);
					s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t8);
					s_Xy[2].y = s_Xy[1].y;
					s_Xy[3].y = s_Xy[0].y;
					s_Uv[0].x = Mathf.Lerp(x, z, t5);
					s_Uv[1].x = s_Uv[0].x;
					s_Uv[2].x = Mathf.Lerp(x, z, t6);
					s_Uv[3].x = s_Uv[2].x;
					s_Uv[0].y = Mathf.Lerp(y, w, t7);
					s_Uv[1].y = Mathf.Lerp(y, w, t8);
					s_Uv[2].y = s_Uv[1].y;
					s_Uv[3].y = s_Uv[0].y;
					float value2 = (m_FillClockwise ? (m_FillAmount * 4f - (float)((l + m_FillOrigin) % 4)) : (m_FillAmount * 4f - (float)(3 - (l + m_FillOrigin) % 4)));
					if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(value2), m_FillClockwise, (l + 2) % 4))
					{
						for (int m = 0; m < 4; m++)
						{
							simpleVert.position = s_Xy[m];
							simpleVert.uv0 = s_Uv[m];
							vbo.Add(simpleVert);
						}
					}
				}
				return;
			}
		}
		for (int n = 0; n < 4; n++)
		{
			simpleVert.position = s_Xy[n];
			simpleVert.uv0 = s_Uv[n];
			vbo.Add(simpleVert);
		}
	}

	private static bool RadialCut(Vector2[] xy, Vector2[] uv, float fill, bool invert, int corner)
	{
		if (fill < 0.001f)
		{
			return false;
		}
		if ((corner & 1) == 1)
		{
			invert = !invert;
		}
		if (!invert && fill > 0.999f)
		{
			return true;
		}
		float num = Mathf.Clamp01(fill);
		if (invert)
		{
			num = 1f - num;
		}
		num *= MathF.PI / 2f;
		float cos = Mathf.Cos(num);
		float sin = Mathf.Sin(num);
		RadialCut(xy, cos, sin, invert, corner);
		RadialCut(uv, cos, sin, invert, corner);
		return true;
	}

	private static void RadialCut(Vector2[] xy, float cos, float sin, bool invert, int corner)
	{
		int num = (corner + 1) % 4;
		int num2 = (corner + 2) % 4;
		int num3 = (corner + 3) % 4;
		if ((corner & 1) == 1)
		{
			if (sin > cos)
			{
				cos /= sin;
				sin = 1f;
				if (invert)
				{
					xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
					xy[num2].x = xy[num].x;
				}
			}
			else if (cos > sin)
			{
				sin /= cos;
				cos = 1f;
				if (!invert)
				{
					xy[num2].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
					xy[num3].y = xy[num2].y;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}
			if (!invert)
			{
				xy[num3].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
			}
			else
			{
				xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
			}
			return;
		}
		if (cos > sin)
		{
			sin /= cos;
			cos = 1f;
			if (!invert)
			{
				xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
				xy[num2].y = xy[num].y;
			}
		}
		else if (sin > cos)
		{
			cos /= sin;
			sin = 1f;
			if (invert)
			{
				xy[num2].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
				xy[num3].x = xy[num2].x;
			}
		}
		else
		{
			cos = 1f;
			sin = 1f;
		}
		if (invert)
		{
			xy[num3].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
		}
		else
		{
			xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
		}
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (m_EventAlphaThreshold >= 1f)
		{
			return true;
		}
		Sprite sprite = overrideSprite;
		if (sprite == null)
		{
			return true;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out var localPoint);
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		localPoint.x += base.rectTransform.pivot.x * pixelAdjustedRect.width;
		localPoint.y += base.rectTransform.pivot.y * pixelAdjustedRect.height;
		localPoint = MapCoordinate(localPoint, pixelAdjustedRect);
		Rect textureRect = sprite.textureRect;
		Vector2 vector = new Vector2(localPoint.x / textureRect.width, localPoint.y / textureRect.height);
		float u = Mathf.Lerp(textureRect.x, textureRect.xMax, vector.x) / (float)sprite.texture.width;
		float v = Mathf.Lerp(textureRect.y, textureRect.yMax, vector.y) / (float)sprite.texture.height;
		try
		{
			return sprite.texture.GetPixelBilinear(u, v).a >= m_EventAlphaThreshold;
		}
		catch (UnityException ex)
		{
			Debug.LogError("Using clickAlphaThreshold lower than 1 on Image whose sprite texture cannot be read. " + ex.Message + " Also make sure to disable sprite packing for this sprite.", this);
			return true;
		}
	}

	private Vector2 MapCoordinate(Vector2 local, Rect rect)
	{
		Rect rect2 = sprite.rect;
		if (type == Type.Simple || type == Type.Filled)
		{
			return new Vector2(local.x * rect2.width / rect.width, local.y * rect2.height / rect.height);
		}
		Vector4 border = sprite.border;
		Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
		for (int i = 0; i < 2; i++)
		{
			if (!(local[i] <= adjustedBorders[i]))
			{
				if (rect.size[i] - local[i] <= adjustedBorders[i + 2])
				{
					local[i] -= rect.size[i] - rect2.size[i];
				}
				else if (type == Type.Sliced)
				{
					float t = Mathf.InverseLerp(adjustedBorders[i], rect.size[i] - adjustedBorders[i + 2], local[i]);
					local[i] = Mathf.Lerp(border[i], rect2.size[i] - border[i + 2], t);
				}
				else
				{
					local[i] -= adjustedBorders[i];
					local[i] = Mathf.Repeat(local[i], rect2.size[i] - border[i] - border[i + 2]);
					local[i] += border[i];
				}
			}
		}
		return local;
	}
}
