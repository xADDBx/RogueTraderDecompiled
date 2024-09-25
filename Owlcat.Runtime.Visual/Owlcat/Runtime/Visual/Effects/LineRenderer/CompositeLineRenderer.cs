using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Core.Utility;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.LineRenderer;

[ExecuteInEditMode]
public class CompositeLineRenderer : ProceduralMesh
{
	private readonly VertexAttributeDescriptor[] m_VertexAttributeDescriptorsUnlit = new VertexAttributeDescriptor[3]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
	};

	private readonly VertexAttributeDescriptor[] m_VertexAttributeDescriptorsLit = new VertexAttributeDescriptor[5]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4),
		new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4),
		new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
	};

	[SerializeField]
	private int m_MaxPositionsPerLine = 5;

	[SerializeField]
	private Gradient m_ColorOverLength = new Gradient();

	[SerializeField]
	private AnimationCurve m_With = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[SerializeField]
	private float m_WidthScale = 1f;

	[SerializeField]
	private LineTextureMode m_TextureMode;

	[SerializeField]
	private LineAlignment m_Alignment;

	[SerializeField]
	private bool m_GenerateLightingData;

	private bool m_MeshAttributesIsDirty;

	private bool m_MeshIndicesIsDirty;

	private bool m_NativeDataIsDirty;

	private bool m_WithCurveIsDirty;

	private bool m_GradientIsDirty;

	private NativeArray<VertexColorUv> m_VerticesUnlit;

	private NativeArray<VertexColorUvNormalTangent> m_VerticesLit;

	private int m_VertexCount;

	private int m_VertexCountActive;

	private int m_IndexCount;

	private int m_IndexCountActive;

	private NativeArray<uint> m_Indices;

	private NativeArray<Bounds> m_Bounds;

	private NativeArray<LineDescriptor> m_LineDescriptors;

	private NativeArray<GeometryLineDescriptor> m_GeometryLineDescriptors;

	private NativeArray<Point> m_Points;

	private JobGradient m_JobColorOverLength;

	private JobAnimationCurve m_JobWidthCurve;

	private int m_LinesCountActive;

	private UpdateGeometryJob m_Job;

	public Func<JobHandle> OnUpdateJobStart;

	public int MaxPositionsPerLine
	{
		get
		{
			return m_MaxPositionsPerLine;
		}
		set
		{
			if (m_MaxPositionsPerLine != value)
			{
				m_MeshAttributesIsDirty = true;
				m_MeshIndicesIsDirty = true;
				m_NativeDataIsDirty = true;
				m_MaxPositionsPerLine = value;
			}
		}
	}

	public LineTextureMode TextureMode
	{
		get
		{
			return m_TextureMode;
		}
		set
		{
			m_TextureMode = value;
		}
	}

	public LineAlignment Alignment
	{
		get
		{
			return m_Alignment;
		}
		set
		{
			m_Alignment = value;
		}
	}

	public bool GenerateLightingData
	{
		get
		{
			return m_GenerateLightingData;
		}
		set
		{
			if (m_GenerateLightingData != value)
			{
				m_GenerateLightingData = value;
				m_MeshAttributesIsDirty = true;
				m_VertexAttributeDescriptors = (m_GenerateLightingData ? m_VertexAttributeDescriptorsLit : m_VertexAttributeDescriptorsUnlit);
				UpdateVerticesArrays(m_VertexCount);
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Initialize();
	}

	private void Initialize()
	{
		m_Job = default(UpdateGeometryJob);
		m_LinesCountActive = 0;
		NativeArrayUtils.IncreaseSize(ref m_LineDescriptors, m_LinesCountActive, NativeArrayOptions.UninitializedMemory);
		m_VertexAttributeDescriptors = (m_GenerateLightingData ? m_VertexAttributeDescriptorsLit : m_VertexAttributeDescriptorsUnlit);
		m_MeshAttributesIsDirty = true;
		m_MeshIndicesIsDirty = true;
		m_NativeDataIsDirty = true;
		m_WithCurveIsDirty = true;
		m_GradientIsDirty = true;
	}

	private void UpdateNativeData()
	{
		if (m_NativeDataIsDirty)
		{
			m_VertexCount = 0;
			m_IndexCount = 0;
			for (int i = 0; i < m_LinesCountActive; i++)
			{
				m_VertexCount += m_MaxPositionsPerLine * 2;
				int num = m_MaxPositionsPerLine - 1;
				m_IndexCount += num * 6;
			}
			UpdateVerticesArrays(m_VertexCount);
			NativeArrayUtils.IncreaseSize(ref m_Indices, m_IndexCount, NativeArrayOptions.UninitializedMemory);
			NativeArrayUtils.IncreaseSize(ref m_Bounds, m_LinesCountActive, NativeArrayOptions.UninitializedMemory);
			NativeArrayUtils.IncreaseSize(ref m_GeometryLineDescriptors, m_LinesCountActive, NativeArrayOptions.UninitializedMemory);
			NativeArrayUtils.IncreaseSize(ref m_Points, m_LinesCountActive * m_MaxPositionsPerLine, NativeArrayOptions.UninitializedMemory);
			if (m_GradientIsDirty)
			{
				m_JobColorOverLength.Dispose();
				m_JobColorOverLength = new JobGradient(m_ColorOverLength);
				m_GradientIsDirty = false;
			}
			if (m_WithCurveIsDirty)
			{
				m_JobWidthCurve.Dispose();
				m_JobWidthCurve = new JobAnimationCurve(m_With);
				m_WithCurveIsDirty = false;
			}
			PrepareGeometryJobData();
			m_NativeDataIsDirty = false;
		}
	}

	private void UpdateVerticesArrays(int vertexCount)
	{
		if (m_GenerateLightingData)
		{
			if (m_VerticesUnlit.IsCreated)
			{
				m_VerticesUnlit.Dispose();
			}
			m_VerticesUnlit = new NativeArray<VertexColorUv>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			NativeArrayUtils.IncreaseSize(ref m_VerticesLit, vertexCount, NativeArrayOptions.UninitializedMemory);
		}
		else
		{
			if (m_VerticesLit.IsCreated)
			{
				m_VerticesLit.Dispose();
			}
			m_VerticesLit = new NativeArray<VertexColorUvNormalTangent>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			NativeArrayUtils.IncreaseSize(ref m_VerticesUnlit, vertexCount, NativeArrayOptions.UninitializedMemory);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_VerticesUnlit.IsCreated)
		{
			m_VerticesUnlit.Dispose();
		}
		if (m_VerticesLit.IsCreated)
		{
			m_VerticesLit.Dispose();
		}
		if (m_Indices.IsCreated)
		{
			m_Indices.Dispose();
		}
		if (m_Bounds.IsCreated)
		{
			m_Bounds.Dispose();
		}
		if (m_GeometryLineDescriptors.IsCreated)
		{
			m_GeometryLineDescriptors.Dispose();
		}
		if (m_LineDescriptors.IsCreated)
		{
			m_LineDescriptors.Dispose();
		}
		if (m_Points.IsCreated)
		{
			m_Points.Dispose();
		}
		m_JobColorOverLength.Dispose();
		m_JobWidthCurve.Dispose();
	}

	protected override JobHandle StartUpdateJob()
	{
		if (OnUpdateJobStart != null)
		{
			return OnUpdateJobStart();
		}
		return default(JobHandle);
	}

	protected override JobHandle StartCameraUpdateGeometryJob(JobHandle dependsOn, Camera camera)
	{
		JobHandle result = dependsOn;
		if (m_HasCameraDependency)
		{
			if (m_NativeDataIsDirty)
			{
				UpdateNativeData();
			}
			m_Job.GeometryLineDescriptors = m_GeometryLineDescriptors;
			m_Job.LineDescriptors = m_LineDescriptors;
			m_Job.Points = m_Points;
			m_Job.VerticesUnlit = m_VerticesUnlit;
			m_Job.VerticesLit = m_VerticesLit;
			m_Job.Indices = m_Indices;
			m_Job.LineBounds = m_Bounds;
			m_Job.ColorOverLength = m_JobColorOverLength;
			m_Job.WidthCurve = m_JobWidthCurve;
			m_Job.WidthScale = m_WidthScale;
			m_Job.Up = new float3(0f, 0f, -1f);
			m_Job.TextureMode = m_TextureMode;
			m_Job.Alignment = m_Alignment;
			m_Job.CameraPosition = camera.transform.position;
			m_Job.GenerateLightingData = m_GenerateLightingData;
			m_Job.MeshIndicesIsDirty = m_MeshIndicesIsDirty;
			m_Job.Space = m_Space;
			m_Job.InvWorld = base.transform.worldToLocalMatrix;
			result = IJobParallelForExtensions.Schedule(m_Job, m_LinesCountActive, 8, dependsOn);
		}
		return result;
	}

	protected override JobHandle StartUpdateGeometryJob(JobHandle dependsOn)
	{
		JobHandle result = dependsOn;
		if (!m_HasCameraDependency)
		{
			if (m_NativeDataIsDirty)
			{
				UpdateNativeData();
			}
			m_Job.GeometryLineDescriptors = m_GeometryLineDescriptors;
			m_Job.LineDescriptors = m_LineDescriptors;
			m_Job.Points = m_Points;
			m_Job.VerticesUnlit = m_VerticesUnlit;
			m_Job.VerticesLit = m_VerticesLit;
			m_Job.Indices = m_Indices;
			m_Job.LineBounds = m_Bounds;
			m_Job.ColorOverLength = m_JobColorOverLength;
			m_Job.WidthCurve = m_JobWidthCurve;
			m_Job.WidthScale = m_WidthScale;
			m_Job.Up = new float3(0f, 0f, -1f);
			m_Job.TextureMode = m_TextureMode;
			m_Job.Alignment = m_Alignment;
			m_Job.GenerateLightingData = m_GenerateLightingData;
			m_Job.MeshIndicesIsDirty = m_MeshIndicesIsDirty;
			m_Job.Space = m_Space;
			m_Job.InvWorld = base.transform.worldToLocalMatrix;
			result = IJobParallelForExtensions.Schedule(m_Job, m_LinesCountActive, 8, dependsOn);
		}
		return result;
	}

	private void PrepareGeometryJobData()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < m_LinesCountActive; i++)
		{
			LineDescriptor lineDescriptor = m_LineDescriptors[i];
			int num3 = math.min(m_MaxPositionsPerLine, lineDescriptor.PositionCount);
			int num4 = num3 * 2;
			int num5 = ((num3 > 1) ? ((num3 - 1) * 6) : 0);
			m_GeometryLineDescriptors[i] = new GeometryLineDescriptor
			{
				IndexCount = num5,
				IndexOffset = num2,
				VertexCount = num4,
				VertexOffset = num
			};
			num += num4;
			num2 += num5;
		}
		m_VertexCountActive = num;
		m_IndexCountActive = num2;
	}

	protected override void AfterUpdateJobComplete()
	{
		if (m_Mesh == null)
		{
			return;
		}
		if (m_GenerateLightingData)
		{
			if (!m_VerticesLit.IsCreated)
			{
				return;
			}
		}
		else if (!m_VerticesUnlit.IsCreated)
		{
			return;
		}
		if (m_MeshAttributesIsDirty)
		{
			m_VertexAttributeDescriptors = (m_GenerateLightingData ? m_VertexAttributeDescriptorsLit : m_VertexAttributeDescriptorsUnlit);
			m_Mesh.SetVertexBufferParams(m_VertexCount, m_VertexAttributeDescriptors);
			m_MeshAttributesIsDirty = false;
		}
		if (m_GenerateLightingData)
		{
			m_Mesh.SetVertexBufferData(m_VerticesLit, 0, 0, m_VertexCountActive, 0, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
		}
		else
		{
			m_Mesh.SetVertexBufferData(m_VerticesUnlit, 0, 0, m_VertexCountActive, 0, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
		}
		if (m_MeshIndicesIsDirty)
		{
			m_Mesh.SetIndexBufferParams(m_IndexCount, IndexFormat.UInt32);
			m_Mesh.SetIndexBufferData(m_Indices, 0, 0, m_IndexCountActive, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
			m_Mesh.SetSubMesh(0, new SubMeshDescriptor(0, m_IndexCountActive), MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
			m_MeshIndicesIsDirty = false;
		}
		if (m_Job.LineBounds.Length > 0)
		{
			Bounds bounds = m_Job.LineBounds[0];
			for (int i = 1; i < m_LinesCountActive; i++)
			{
				bounds.Encapsulate(m_Job.LineBounds[i]);
			}
			m_Mesh.bounds = bounds;
		}
		else
		{
			m_Mesh.bounds = default(Bounds);
		}
	}

	protected override bool GetCameraDependency()
	{
		return m_Alignment == LineAlignment.Camera;
	}

	public override void SetDirtyFromEditor()
	{
		m_MeshAttributesIsDirty = true;
		m_MeshIndicesIsDirty = true;
		m_NativeDataIsDirty = true;
		m_WithCurveIsDirty = true;
		m_GradientIsDirty = true;
	}

	internal void SetLineDescriptors(LineDescriptor[] lineDescriptors)
	{
		m_LinesCountActive = lineDescriptors.Length;
		NativeArrayUtils.IncreaseSize(ref m_LineDescriptors, lineDescriptors.Length, NativeArrayOptions.UninitializedMemory);
		m_LineDescriptors.CopyFrom(lineDescriptors);
		m_MeshAttributesIsDirty = true;
		m_MeshIndicesIsDirty = true;
		m_NativeDataIsDirty = true;
	}

	internal void SetLineDescriptors(List<LineDescriptor> lineDescriptors)
	{
		m_LinesCountActive = lineDescriptors.Count;
		NativeArrayUtils.IncreaseSize(ref m_LineDescriptors, lineDescriptors.Count, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < lineDescriptors.Count; i++)
		{
			m_LineDescriptors[i] = lineDescriptors[i];
		}
		m_MeshAttributesIsDirty = true;
		m_MeshIndicesIsDirty = true;
		m_NativeDataIsDirty = true;
	}

	public JobHandle ScheduleUpdateJob<T>(ref T job, int innerLoopBatchCount) where T : struct, IJobLineRenderer
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled)
		{
			return default(JobHandle);
		}
		UpdateNativeData();
		job.Points = m_Points;
		job.Lines = m_LineDescriptors;
		return IJobParallelForExtensions.Schedule(job, m_LinesCountActive, innerLoopBatchCount);
	}

	public override string GetStats()
	{
		return $"Vertex count: {m_VertexCount}\nIndex count: {m_IndexCount}\nTriangle count: {m_IndexCount / 3}\nVertex active: {m_VertexCountActive}\nIndex active: {m_IndexCountActive}";
	}
}
