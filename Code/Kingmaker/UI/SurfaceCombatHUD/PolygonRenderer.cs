using System;
using System.Collections.Generic;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class PolygonRenderer : MonoBehaviour
{
	private class PolygonRenderData
	{
		public Vector4[] Vertices;

		public Color Color;
	}

	private const int k_totalVertexNum = 1023;

	private const int k_totalPolygonNum = 20;

	private readonly Vector4[] m_vertexArray = new Vector4[1023];

	private readonly float[] m_numVertexArray = new float[20];

	private readonly Vector4[] m_colorArray = new Vector4[20];

	private Renderer m_Renderer;

	private readonly Dictionary<string, PolygonRenderData> m_Polygons = new Dictionary<string, PolygonRenderData>();

	private void Start()
	{
		m_Renderer = GetComponent<Renderer>();
	}

	public void SetPolygon(string name, Polygon polygon, Color color)
	{
		if (m_Polygons.ContainsKey(name))
		{
			m_Polygons[name].Vertices = polygon.Vertices;
			m_Polygons[name].Color = color;
		}
		else
		{
			m_Polygons.Add(name, new PolygonRenderData
			{
				Vertices = polygon.Vertices,
				Color = color
			});
		}
		UpdatePolygons();
	}

	public void RemovePolygon(string name)
	{
		m_Polygons.Remove(name);
		UpdatePolygons();
	}

	private void UpdatePolygons()
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		int num = 0;
		int num2 = 0;
		foreach (PolygonRenderData value in m_Polygons.Values)
		{
			if (value.Vertices != null)
			{
				int num3 = value.Vertices.Length;
				if (num2 >= 20 || num + num3 >= 1023)
				{
					break;
				}
				Array.Copy(value.Vertices, 0, m_vertexArray, num, num3);
				m_numVertexArray[num2] = num3;
				m_colorArray[num2] = value.Color;
				num += num3;
				num2++;
			}
		}
		materialPropertyBlock.SetVectorArray(ShaderProps._Polygon, m_vertexArray);
		materialPropertyBlock.SetFloat(ShaderProps._NumPolygons, num2);
		materialPropertyBlock.SetFloatArray(ShaderProps._NumVertices, m_numVertexArray);
		materialPropertyBlock.SetVectorArray(ShaderProps._Colors, m_colorArray);
		GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
	}
}
