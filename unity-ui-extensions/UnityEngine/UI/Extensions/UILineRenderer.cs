using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/UILineRenderer")]
public class UILineRenderer : UIPrimitiveBase
{
	private enum SegmentType
	{
		Start,
		Middle,
		End
	}

	public enum JoinType
	{
		Bevel,
		Miter
	}

	public enum BezierType
	{
		None,
		Quick,
		Basic,
		Improved
	}

	private const float MIN_MITER_JOIN = MathF.PI / 12f;

	private const float MIN_BEVEL_NICE_JOIN = MathF.PI / 6f;

	private static readonly Vector2 UV_TOP_LEFT = Vector2.zero;

	private static readonly Vector2 UV_BOTTOM_LEFT = new Vector2(0f, 1f);

	private static readonly Vector2 UV_TOP_CENTER = new Vector2(0.5f, 0f);

	private static readonly Vector2 UV_BOTTOM_CENTER = new Vector2(0.5f, 1f);

	private static readonly Vector2 UV_TOP_RIGHT = new Vector2(1f, 0f);

	private static readonly Vector2 UV_BOTTOM_RIGHT = new Vector2(1f, 1f);

	private static readonly Vector2[] startUvs = new Vector2[4] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER, UV_TOP_CENTER };

	private static readonly Vector2[] middleUvs = new Vector2[4] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_CENTER, UV_TOP_CENTER };

	private static readonly Vector2[] endUvs = new Vector2[4] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };

	[SerializeField]
	private Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

	[SerializeField]
	private Vector2[] m_points;

	public float LineThickness = 2f;

	public bool UseMargins;

	public Vector2 Margin;

	public bool relativeSize;

	public bool LineList;

	public bool LineCaps;

	public JoinType LineJoins;

	public BezierType BezierMode;

	public int BezierSegmentsPerCurve = 10;

	public Rect uvRect
	{
		get
		{
			return m_UVRect;
		}
		set
		{
			if (!(m_UVRect == value))
			{
				m_UVRect = value;
				SetVerticesDirty();
			}
		}
	}

	public Vector2[] Points
	{
		get
		{
			return m_points;
		}
		set
		{
			if (m_points != value)
			{
				m_points = value;
				SetAllDirty();
			}
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if (m_points == null)
		{
			return;
		}
		Vector2[] array = m_points;
		if (BezierMode != 0 && m_points.Length > 3)
		{
			BezierPath bezierPath = new BezierPath();
			bezierPath.SetControlPoints(array);
			bezierPath.SegmentsPerCurve = BezierSegmentsPerCurve;
			array = (BezierMode switch
			{
				BezierType.Basic => bezierPath.GetDrawingPoints0(), 
				BezierType.Improved => bezierPath.GetDrawingPoints1(), 
				_ => bezierPath.GetDrawingPoints2(), 
			}).ToArray();
		}
		float num = base.rectTransform.rect.width;
		float num2 = base.rectTransform.rect.height;
		float num3 = (0f - base.rectTransform.pivot.x) * base.rectTransform.rect.width;
		float num4 = (0f - base.rectTransform.pivot.y) * base.rectTransform.rect.height;
		if (!relativeSize)
		{
			num = 1f;
			num2 = 1f;
		}
		if (UseMargins)
		{
			num -= Margin.x;
			num2 -= Margin.y;
			num3 += Margin.x / 2f;
			num4 += Margin.y / 2f;
		}
		vh.Clear();
		List<UIVertex[]> list = new List<UIVertex[]>();
		if (LineList)
		{
			for (int i = 1; i < array.Length; i += 2)
			{
				Vector2 vector = array[i - 1];
				Vector2 vector2 = array[i];
				vector = new Vector2(vector.x * num + num3, vector.y * num2 + num4);
				vector2 = new Vector2(vector2.x * num + num3, vector2.y * num2 + num4);
				if (LineCaps)
				{
					list.Add(CreateLineCap(vector, vector2, SegmentType.Start));
				}
				list.Add(CreateLineSegment(vector, vector2, SegmentType.Middle));
				if (LineCaps)
				{
					list.Add(CreateLineCap(vector, vector2, SegmentType.End));
				}
			}
		}
		else
		{
			for (int j = 1; j < array.Length; j++)
			{
				Vector2 vector3 = array[j - 1];
				Vector2 vector4 = array[j];
				vector3 = new Vector2(vector3.x * num + num3, vector3.y * num2 + num4);
				vector4 = new Vector2(vector4.x * num + num3, vector4.y * num2 + num4);
				if (LineCaps && j == 1)
				{
					list.Add(CreateLineCap(vector3, vector4, SegmentType.Start));
				}
				list.Add(CreateLineSegment(vector3, vector4, SegmentType.Middle));
				if (LineCaps && j == array.Length - 1)
				{
					list.Add(CreateLineCap(vector3, vector4, SegmentType.End));
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			if (!LineList && k < list.Count - 1)
			{
				Vector3 vector5 = list[k][1].position - list[k][2].position;
				Vector3 vector6 = list[k + 1][2].position - list[k + 1][1].position;
				float num5 = Vector2.Angle(vector5, vector6) * (MathF.PI / 180f);
				float num6 = Mathf.Sign(Vector3.Cross(vector5.normalized, vector6.normalized).z);
				float num7 = LineThickness / (2f * Mathf.Tan(num5 / 2f));
				Vector3 position = list[k][2].position - vector5.normalized * num7 * num6;
				Vector3 position2 = list[k][3].position + vector5.normalized * num7 * num6;
				JoinType joinType = LineJoins;
				if (joinType == JoinType.Miter)
				{
					if (num7 < vector5.magnitude / 2f && num7 < vector6.magnitude / 2f && num5 > MathF.PI / 12f)
					{
						list[k][2].position = position;
						list[k][3].position = position2;
						list[k + 1][0].position = position2;
						list[k + 1][1].position = position;
					}
					else
					{
						joinType = JoinType.Bevel;
					}
				}
				if (joinType == JoinType.Bevel)
				{
					if (num7 < vector5.magnitude / 2f && num7 < vector6.magnitude / 2f && num5 > MathF.PI / 6f)
					{
						if (num6 < 0f)
						{
							list[k][2].position = position;
							list[k + 1][1].position = position;
						}
						else
						{
							list[k][3].position = position2;
							list[k + 1][0].position = position2;
						}
					}
					UIVertex[] verts = new UIVertex[4]
					{
						list[k][2],
						list[k][3],
						list[k + 1][0],
						list[k + 1][1]
					};
					vh.AddUIVertexQuad(verts);
				}
			}
			vh.AddUIVertexQuad(list[k]);
		}
	}

	private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
	{
		switch (type)
		{
		case SegmentType.Start:
		{
			Vector2 start2 = start - (end - start).normalized * LineThickness / 2f;
			return CreateLineSegment(start2, start, SegmentType.Start);
		}
		case SegmentType.End:
		{
			Vector2 end2 = end + (end - start).normalized * LineThickness / 2f;
			return CreateLineSegment(end, end2, SegmentType.End);
		}
		default:
			Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be SegmentType.Start or SegmentType.End");
			return null;
		}
	}

	private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type)
	{
		Vector2[] uvs = middleUvs;
		switch (type)
		{
		case SegmentType.Start:
			uvs = startUvs;
			break;
		case SegmentType.End:
			uvs = endUvs;
			break;
		}
		Vector2 vector = new Vector2(start.y - end.y, end.x - start.x).normalized * LineThickness / 2f;
		Vector2 vector2 = start - vector;
		Vector2 vector3 = start + vector;
		Vector2 vector4 = end + vector;
		Vector2 vector5 = end - vector;
		return SetVbo(new Vector2[4] { vector2, vector3, vector4, vector5 }, uvs);
	}
}
