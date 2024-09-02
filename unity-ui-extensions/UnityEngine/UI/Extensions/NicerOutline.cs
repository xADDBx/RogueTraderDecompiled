using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Effects/Extensions/Nicer Outline")]
public class NicerOutline : BaseMeshEffect
{
	[SerializeField]
	private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);

	[SerializeField]
	private Vector2 m_EffectDistance = new Vector2(1f, -1f);

	[SerializeField]
	private bool m_UseGraphicAlpha = true;

	public Color effectColor
	{
		get
		{
			return m_EffectColor;
		}
		set
		{
			m_EffectColor = value;
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	public Vector2 effectDistance
	{
		get
		{
			return m_EffectDistance;
		}
		set
		{
			if (value.x > 600f)
			{
				value.x = 600f;
			}
			if (value.x < -600f)
			{
				value.x = -600f;
			}
			if (value.y > 600f)
			{
				value.y = 600f;
			}
			if (value.y < -600f)
			{
				value.y = -600f;
			}
			if (!(m_EffectDistance == value))
			{
				m_EffectDistance = value;
				if (base.graphic != null)
				{
					base.graphic.SetVerticesDirty();
				}
			}
		}
	}

	public bool useGraphicAlpha
	{
		get
		{
			return m_UseGraphicAlpha;
		}
		set
		{
			m_UseGraphicAlpha = value;
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	protected void ApplyShadowZeroAlloc(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
	{
		int num = verts.Count * 2;
		if (verts.Capacity < num)
		{
			verts.Capacity = num;
		}
		for (int i = start; i < end; i++)
		{
			UIVertex uIVertex = verts[i];
			verts.Add(uIVertex);
			Vector3 position = uIVertex.position;
			position.x += x;
			position.y += y;
			uIVertex.position = position;
			Color32 color2 = color;
			if (m_UseGraphicAlpha)
			{
				color2.a = (byte)(color2.a * verts[i].color.a / 255);
			}
			uIVertex.color = color2;
			verts[i] = uIVertex;
		}
	}

	protected void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
	{
		int num = verts.Count * 2;
		if (verts.Capacity < num)
		{
			verts.Capacity = num;
		}
		ApplyShadowZeroAlloc(verts, color, start, end, x, y);
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (IsActive())
		{
			List<UIVertex> list = new List<UIVertex>();
			vh.GetUIVertexStream(list);
			Text component = GetComponent<Text>();
			float num = 1f;
			if ((bool)component && component.resizeTextForBestFit)
			{
				num = (float)component.cachedTextGenerator.fontSizeUsedForBestFit / (float)(component.resizeTextMaxSize - 1);
			}
			float num2 = effectDistance.x * num;
			float num3 = effectDistance.y * num;
			int start = 0;
			int count = list.Count;
			ApplyShadow(list, effectColor, start, list.Count, num2, num3);
			start = count;
			int count2 = list.Count;
			ApplyShadow(list, effectColor, start, list.Count, num2, 0f - num3);
			start = count2;
			int count3 = list.Count;
			ApplyShadow(list, effectColor, start, list.Count, 0f - num2, num3);
			start = count3;
			int count4 = list.Count;
			ApplyShadow(list, effectColor, start, list.Count, 0f - num2, 0f - num3);
			start = count4;
			int count5 = list.Count;
			ApplyShadow(list, effectColor, start, list.Count, num2, 0f);
			start = count5;
			int count6 = list.Count;
			ApplyShadow(list, effectColor, start, list.Count, 0f - num2, 0f);
			start = count6;
			int count7 = list.Count;
			ApplyShadow(list, effectColor, start, list.Count, 0f, num3);
			start = count7;
			_ = list.Count;
			ApplyShadow(list, effectColor, start, list.Count, 0f, 0f - num3);
			vh.Clear();
			vh.AddUIVertexTriangleStream(list);
		}
	}
}
