using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Owlcat.Core.Overlays;

public class Overlay
{
	public readonly string Name;

	private readonly List<Label> m_LabelDefinitions = new List<Label>();

	private readonly List<Graph> m_GraphDefinitions = new List<Graph>();

	public bool SharedGraphBounds;

	public bool StackGraphs;

	public List<Label> Labels => m_LabelDefinitions;

	public List<Graph> Graphs => m_GraphDefinitions;

	public Overlay(string name, params OverlayElement[] elements)
	{
		Name = name;
		m_LabelDefinitions.AddRange(elements.OfType<Label>());
		m_GraphDefinitions.AddRange(elements.OfType<Graph>());
	}

	protected void Add(Label l)
	{
		m_LabelDefinitions.Add(l);
	}

	protected void Add(Graph g)
	{
		m_GraphDefinitions.Add(g);
	}

	public void ToggleGraph(string name)
	{
		Graph graph = m_GraphDefinitions.FirstOrDefault((Graph g) => g.Name == name);
		if (graph != null)
		{
			graph.Hidden = !graph.Hidden;
		}
	}

	public void Draw()
	{
		float num = DrawLabels();
		if (DrawGraphsLegend(num, out var height) == 0)
		{
			return;
		}
		float num2 = height + 21;
		double min = 0.0;
		double max = 0.0;
		if (SharedGraphBounds)
		{
			min = (from g in m_GraphDefinitions
				where !g.Hidden
				select g.GetMinValue()).Min();
			max = (from g in m_GraphDefinitions
				where !g.Hidden
				select g.GetMaxValue()).Max();
		}
		Rect r = new Rect(num, num2, (float)Screen.width - num, Mathf.Min(350f, (float)Screen.height - num2));
		DrawGraphs(r, min, max);
		if (SharedGraphBounds)
		{
			GUI.Label(new Rect(r.xMin, r.yMin, 64f, 20f), max.ToString("0.00"));
			GUI.Label(new Rect(r.xMin, r.yMax - 20f, 64f, 20f), min.ToString("0.00"));
			return;
		}
		float num3 = r.xMin;
		_ = GUI.color;
		foreach (Graph graphDefinition in m_GraphDefinitions)
		{
			if (!graphDefinition.Hidden)
			{
				GUI.color = graphDefinition.Color;
				GUI.Label(new Rect(num3, r.yMin, 64f, 20f), graphDefinition.GetMaxValue().ToString("0.00"));
				GUI.Label(new Rect(num3, r.yMax - 20f, 64f, 20f), graphDefinition.GetMinValue().ToString("0.00"));
				num3 += 64f;
			}
		}
	}

	private void DrawGraphs(Rect r, double min, double max)
	{
		Color color = GUI.color;
		GUI.color = new Color(0f, 0f, 0f, OverlayService.Instance.DarkenBackground ? 0.5f : 0f);
		GUI.DrawTexture(r, Texture2D.whiteTexture);
		GUI.color = color;
		foreach (Graph graphDefinition in m_GraphDefinitions)
		{
			if (!graphDefinition.Hidden)
			{
				if (SharedGraphBounds)
				{
					graphDefinition.DrawGraph(r, min, max);
				}
				else
				{
					graphDefinition.DrawGraph(r);
				}
			}
		}
	}

	private int DrawGraphsLegend(float right, out int height)
	{
		float num = (float)Screen.width - right;
		int num2 = 0;
		height = 0;
		int num3 = 0;
		foreach (Graph graphDefinition in m_GraphDefinitions)
		{
			if (!graphDefinition.Hidden)
			{
				int legendWidth = graphDefinition.LegendWidth;
				if (num2 > 0 && !((float)legendWidth < num - (float)num2))
				{
					height += 20;
					num2 = 0;
				}
				graphDefinition.DrawLegend((float)num2 + right, height);
				num3++;
				num2 += legendWidth;
			}
		}
		return num3;
	}

	private float DrawLabels()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (Label labelDefinition in m_LabelDefinitions)
		{
			if (!labelDefinition.Hidden)
			{
				float val = labelDefinition.Draw(num);
				num += 19f;
				num2 = Math.Max(num2, val);
				if (labelDefinition.AddSeparator)
				{
					num += 10f;
				}
			}
		}
		return num2;
	}

	public void SampleGraphs()
	{
		m_GraphDefinitions.ForEach(delegate(Graph g)
		{
			g.Sample();
		});
	}
}
