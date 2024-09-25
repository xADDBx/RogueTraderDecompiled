using System;
using System.Collections.Generic;
using System.Diagnostics;
using Owlcat.Runtime.Core.Utility;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.LineRenderer;

[RequireComponent(typeof(CompositeLineRenderer))]
[ExecuteInEditMode]
public class CompositeLine : MonoBehaviour
{
	[SerializeField]
	private List<Line> m_Lines = new List<Line>
	{
		new Line
		{
			WidthScale = 1f,
			Positions = new float3[2]
			{
				new float3(0f, 0f, 0f),
				new float3(1f, 0f, 0f)
			}
		}
	};

	private CompositeLineRenderer m_LineRenderer;

	private bool m_IsDirty;

	private NativeArray<Point> m_Points;

	private void OnEnable()
	{
		m_LineRenderer = GetComponent<CompositeLineRenderer>();
		CompositeLineRenderer lineRenderer = m_LineRenderer;
		lineRenderer.OnUpdateJobStart = (Func<JobHandle>)Delegate.Combine(lineRenderer.OnUpdateJobStart, new Func<JobHandle>(OnUpdateJobStart));
		m_IsDirty = true;
	}

	private void OnDisable()
	{
		CompositeLineRenderer lineRenderer = m_LineRenderer;
		lineRenderer.OnUpdateJobStart = (Func<JobHandle>)Delegate.Remove(lineRenderer.OnUpdateJobStart, new Func<JobHandle>(OnUpdateJobStart));
		if (m_Points.IsCreated)
		{
			m_Points.Dispose();
		}
	}

	private JobHandle OnUpdateJobStart()
	{
		JobHandle result = default(JobHandle);
		if (m_IsDirty)
		{
			UpdateNativeData();
			CompositeLineUpdateJob job = default(CompositeLineUpdateJob);
			job.InputPoints = m_Points;
			result = m_LineRenderer.ScheduleUpdateJob(ref job, 8);
			m_IsDirty = false;
		}
		return result;
	}

	public void SetLines(List<Line> lines)
	{
		m_Lines.Clear();
		m_Lines.AddRange(lines);
		m_IsDirty = true;
	}

	private void UpdateNativeData()
	{
		LineDescriptor[] array = new LineDescriptor[m_Lines.Count];
		int num = 0;
		for (int i = 0; i < m_Lines.Count; i++)
		{
			Line line = m_Lines[i];
			array[i] = new LineDescriptor
			{
				PositionCount = line.Positions.Length,
				PositionsOffset = num,
				UvOffset = line.UvOffset,
				WidthScale = line.WidthScale
			};
			num += line.Positions.Length;
		}
		NativeArrayUtils.IncreaseSize(ref m_Points, num, NativeArrayOptions.UninitializedMemory);
		for (int j = 0; j < m_Lines.Count; j++)
		{
			Line line2 = m_Lines[j];
			LineDescriptor lineDescriptor = array[j];
			for (int k = 0; k < line2.Positions.Length; k++)
			{
				m_Points[lineDescriptor.PositionsOffset + k] = new Point
				{
					Position = line2.Positions[k],
					Alpha = 1f
				};
			}
		}
		m_LineRenderer.SetLineDescriptors(array);
	}

	public void GetLines(List<Line> lines)
	{
		lines.Clear();
		lines.AddRange(m_Lines);
	}

	[Conditional("UNITY_EDITOR")]
	public void SetDirtyFromEditor()
	{
		m_IsDirty = true;
	}
}
