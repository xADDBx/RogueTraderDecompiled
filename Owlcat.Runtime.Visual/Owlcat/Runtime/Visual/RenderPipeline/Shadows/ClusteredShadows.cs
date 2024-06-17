using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Shadows;

public class ClusteredShadows
{
	public const int kMaxShadowEntries = 64;

	public const int kMaxShadowMatrices = 256;

	private const float kFov0 = 143.9857f;

	private const float kFov1 = 125.26439f;

	private const float kResolutionReducingDelta = 128f;

	private const int kMaxDirectionalShadowsCount = 4;

	private const int kMaxDirectionalCascadesCount = 4;

	private float4x4[] m_PointScaleMatrices = new float4x4[4]
	{
		float4x4.Scale(-1f, 1f, 1f),
		float4x4.Scale(1f, -1f, 1f),
		float4x4.Scale(-1f, 1f, 1f),
		float4x4.Scale(1f, -1f, 1f)
	};

	private float4x4[] m_PointShadowProjMatrices = new float4x4[2];

	private readonly float4x4[] m_TetrahedronMatrices = new float4x4[4];

	private float4x4[] m_PointLightTexMatrices = new float4x4[4];

	private readonly Rect m_NullRect = new Rect(0f, 0f, 0f, 0f);

	private Vector4[] m_PointLightClips = new Vector4[8];

	private Plane[] m_PointSplitPlanes = new Plane[3];

	private Vector4[] m_DirectionalLightClips4Cascades = new Vector4[8];

	private Matrix4x4[] m_DirectionalLightTexMatrices4Cascades = new Matrix4x4[4];

	private Vector4[] m_DirectionalLightClips3Cascades = new Vector4[6];

	private Matrix4x4[] m_DirectionalLightTexMatrices3Cascades = new Matrix4x4[3];

	private Vector4[] m_DirectionalLightClips2Cascades = new Vector4[4];

	private Matrix4x4[] m_DirectionalLightTexMatrices2Cascades = new Matrix4x4[2];

	private Matrix4x4[] m_DirectionalLightTexMatrices1Cascades = new Matrix4x4[1] { Matrix4x4.identity };

	private Vector4[] m_ShadowDirLightCascadeSpheres = new Vector4[16];

	private Vector4[] m_ShadowDirLightCascadeSpheresRadii = new Vector4[4];

	private Dictionary<int, int> m_ShadowIndicesMap = new Dictionary<int, int>();

	private List<ShadowmapEntry> m_Entries;

	private MaxRectsBinPack m_Packer;

	private List<ShadowData> m_ShadowDataList = new List<ShadowData>();

	private List<ShadowMatrix> m_ShadowMatrices = new List<ShadowMatrix>();

	private ComputeBuffer m_ShadowDataBuffer;

	private ComputeBuffer m_ShadowMatricesBuffer;

	public List<ShadowmapEntry> Entries => m_Entries;

	public List<ShadowMatrix> ShadowMatrices => m_ShadowMatrices;

	public List<ShadowData> ShadowDataList => m_ShadowDataList;

	public Dictionary<int, int> ShadowIndicesMap => m_ShadowIndicesMap;

	public Vector4[] PointLightClips => m_PointLightClips;

	public ComputeBuffer ShadowDataBuffer => m_ShadowDataBuffer;

	public ComputeBuffer ShadowMatricesBuffer => m_ShadowMatricesBuffer;

	public ClusteredShadows()
	{
		m_ShadowDataBuffer = new ComputeBuffer(64, Marshal.SizeOf(typeof(ShadowData)), ComputeBufferType.Structured);
		m_ShadowDataBuffer.name = "Shadow Data Buffer";
		m_ShadowMatricesBuffer = new ComputeBuffer(256, Marshal.SizeOf(typeof(ShadowMatrix)), ComputeBufferType.Structured);
		m_ShadowMatricesBuffer.name = "Shadow Matrices Buffer";
		m_Entries = new List<ShadowmapEntry>();
		m_Packer = new MaxRectsBinPack(16, 16, rotations: false);
		m_PointLightClips[0] = CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(-1f, 1f));
		m_PointLightClips[1] = CalculateLineEquationCoeffs(new Vector2(1f, 1f), new Vector2(0f, 0f));
		m_PointLightClips[2] = CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(1f, 1f));
		m_PointLightClips[3] = CalculateLineEquationCoeffs(new Vector2(1f, -1f), new Vector2(0f, 0f));
		m_PointLightClips[4] = CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(1f, -1f));
		m_PointLightClips[5] = CalculateLineEquationCoeffs(new Vector2(-1f, -1f), new Vector2(0f, 0f));
		m_PointLightClips[6] = CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(-1f, -1f));
		m_PointLightClips[7] = CalculateLineEquationCoeffs(new Vector2(-1f, 1f), new Vector2(0f, 0f));
		m_PointLightTexMatrices[0].c0 = new float4(1f, 0f, 0f, 0f);
		m_PointLightTexMatrices[0].c1 = new float4(0f, 0.5f, 0f, 0f);
		m_PointLightTexMatrices[0].c2 = new float4(0f, 0f, 1f, 0f);
		m_PointLightTexMatrices[0].c3 = new float4(0f, 0.5f, 0f, 1f);
		m_PointLightTexMatrices[1].c0 = new float4(0.5f, 0f, 0f, 0f);
		m_PointLightTexMatrices[1].c1 = new float4(0f, 1f, 0f, 0f);
		m_PointLightTexMatrices[1].c2 = new float4(0f, 0f, 1f, 0f);
		m_PointLightTexMatrices[1].c3 = new float4(0.5f, 0f, 0f, 1f);
		m_PointLightTexMatrices[2].c0 = new float4(1f, 0f, 0f, 0f);
		m_PointLightTexMatrices[2].c1 = new float4(0f, 0.5f, 0f, 0f);
		m_PointLightTexMatrices[2].c2 = new float4(0f, 0f, 1f, 0f);
		m_PointLightTexMatrices[2].c3 = new float4(0f, -0.5f, 0f, 1f);
		m_PointLightTexMatrices[3].c0 = new float4(0.5f, 0f, 0f, 0f);
		m_PointLightTexMatrices[3].c1 = new float4(0f, 1f, 0f, 0f);
		m_PointLightTexMatrices[3].c2 = new float4(0f, 0f, 1f, 0f);
		m_PointLightTexMatrices[3].c3 = new float4(-0.5f, 0f, 0f, 1f);
		m_DirectionalLightClips4Cascades[0] = CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(-1f, 0f));
		m_DirectionalLightClips4Cascades[1] = CalculateLineEquationCoeffs(new Vector2(0f, 1f), new Vector2(0f, 0f));
		m_DirectionalLightClips4Cascades[2] = CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(0f, 1f));
		m_DirectionalLightClips4Cascades[3] = CalculateLineEquationCoeffs(new Vector2(1f, 0f), new Vector2(0f, 0f));
		m_DirectionalLightClips4Cascades[4] = CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(1f, 0f));
		m_DirectionalLightClips4Cascades[5] = CalculateLineEquationCoeffs(new Vector2(0f, -1f), new Vector2(0f, 0f));
		m_DirectionalLightClips4Cascades[6] = CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(0f, -1f));
		m_DirectionalLightClips4Cascades[7] = CalculateLineEquationCoeffs(new Vector2(-1f, 0f), new Vector2(0f, 0f));
		m_DirectionalLightClips3Cascades[0] = CalculateLineEquationCoeffs(new Vector2(-1f, -1f), new Vector2(-1f, 1f));
		m_DirectionalLightClips3Cascades[1] = CalculateLineEquationCoeffs(new Vector2(-0.33333f, 1f), new Vector2(-0.33333f, -1f));
		m_DirectionalLightClips3Cascades[2] = CalculateLineEquationCoeffs(new Vector2(-0.33333f, -1f), new Vector2(-0.33333f, 1f));
		m_DirectionalLightClips3Cascades[3] = CalculateLineEquationCoeffs(new Vector2(0.33333f, 1f), new Vector2(0.33333f, -1f));
		m_DirectionalLightClips3Cascades[4] = CalculateLineEquationCoeffs(new Vector2(0.33333f, -1f), new Vector2(0.33333f, 1f));
		m_DirectionalLightClips3Cascades[5] = CalculateLineEquationCoeffs(new Vector2(1f, 1f), new Vector2(1f, -1f));
		m_DirectionalLightClips2Cascades[0] = CalculateLineEquationCoeffs(new Vector2(-1f, -1f), new Vector2(-1f, 1f));
		m_DirectionalLightClips2Cascades[1] = CalculateLineEquationCoeffs(new Vector2(0f, 1f), new Vector2(0f, -1f));
		m_DirectionalLightClips2Cascades[2] = CalculateLineEquationCoeffs(new Vector2(0f, -1f), new Vector2(0f, 1f));
		m_DirectionalLightClips2Cascades[3] = CalculateLineEquationCoeffs(new Vector2(1f, 1f), new Vector2(1f, -1f));
		m_DirectionalLightTexMatrices4Cascades[0].SetColumn(0, new Vector4(0.5f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices4Cascades[0].SetColumn(1, new Vector4(0f, 0.5f, 0f, 0f));
		m_DirectionalLightTexMatrices4Cascades[0].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices4Cascades[0].SetColumn(3, new Vector4(-0.5f, 0.5f, 0f, 1f));
		m_DirectionalLightTexMatrices4Cascades[1].SetColumn(0, new Vector4(0.5f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices4Cascades[1].SetColumn(1, new Vector4(0f, 0.5f, 0f, 0f));
		m_DirectionalLightTexMatrices4Cascades[1].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices4Cascades[1].SetColumn(3, new Vector4(0.5f, 0.5f, 0f, 1f));
		m_DirectionalLightTexMatrices4Cascades[2].SetColumn(0, new Vector4(0.5f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices4Cascades[2].SetColumn(1, new Vector4(0f, 0.5f, 0f, 0f));
		m_DirectionalLightTexMatrices4Cascades[2].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices4Cascades[2].SetColumn(3, new Vector4(0.5f, -0.5f, 0f, 1f));
		m_DirectionalLightTexMatrices4Cascades[3].SetColumn(0, new Vector4(0.5f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices4Cascades[3].SetColumn(1, new Vector4(0f, 0.5f, 0f, 0f));
		m_DirectionalLightTexMatrices4Cascades[3].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices4Cascades[3].SetColumn(3, new Vector4(-0.5f, -0.5f, 0f, 1f));
		m_DirectionalLightTexMatrices3Cascades[0].SetColumn(0, new Vector4(0.33333f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices3Cascades[0].SetColumn(1, new Vector4(0f, 1f, 0f, 0f));
		m_DirectionalLightTexMatrices3Cascades[0].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices3Cascades[0].SetColumn(3, new Vector4(-0.66666f, 0f, 0f, 1f));
		m_DirectionalLightTexMatrices3Cascades[1].SetColumn(0, new Vector4(0.33333f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices3Cascades[1].SetColumn(1, new Vector4(0f, 1f, 0f, 0f));
		m_DirectionalLightTexMatrices3Cascades[1].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices3Cascades[1].SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
		m_DirectionalLightTexMatrices3Cascades[2].SetColumn(0, new Vector4(0.33333f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices3Cascades[2].SetColumn(1, new Vector4(0f, 1f, 0f, 0f));
		m_DirectionalLightTexMatrices3Cascades[2].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices3Cascades[2].SetColumn(3, new Vector4(0.66666f, 0f, 0f, 1f));
		m_DirectionalLightTexMatrices2Cascades[0].SetColumn(0, new Vector4(0.5f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices2Cascades[0].SetColumn(1, new Vector4(0f, 1f, 0f, 0f));
		m_DirectionalLightTexMatrices2Cascades[0].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices2Cascades[0].SetColumn(3, new Vector4(-0.5f, 0f, 0f, 1f));
		m_DirectionalLightTexMatrices2Cascades[1].SetColumn(0, new Vector4(0.5f, 0f, 0f, 0f));
		m_DirectionalLightTexMatrices2Cascades[1].SetColumn(1, new Vector4(0f, 1f, 0f, 0f));
		m_DirectionalLightTexMatrices2Cascades[1].SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
		m_DirectionalLightTexMatrices2Cascades[1].SetColumn(3, new Vector4(0.5f, 0f, 0f, 1f));
		CalculateTetrahedronMatrices();
	}

	internal void Dispose()
	{
		if (m_ShadowDataBuffer != null)
		{
			m_ShadowDataBuffer.Release();
		}
		if (m_ShadowMatricesBuffer != null)
		{
			m_ShadowMatricesBuffer.Release();
		}
	}

	internal void Setup(ref RenderingData renderingData)
	{
		ProcessRequests(ref renderingData);
		UpdateBuffers();
	}

	private void UpdateBuffers()
	{
		m_ShadowDataList.Clear();
		m_ShadowIndicesMap.Clear();
		for (int i = 0; i < m_Entries.Count; i++)
		{
			ShadowmapEntry shadowmapEntry = m_Entries[i];
			m_ShadowIndicesMap[shadowmapEntry.LightIndex] = i;
			ShadowData item = default(ShadowData);
			item.atlasScaleOffset = shadowmapEntry.ScaleOffset;
			item.matrixIndices = new Vector4(shadowmapEntry.MatrixIndices[0], shadowmapEntry.MatrixIndices[1], shadowmapEntry.MatrixIndices[2], shadowmapEntry.MatrixIndices[3]);
			item.shadowFlags = (int)shadowmapEntry.ShadowFlags;
			item.screenSpaceMask = shadowmapEntry.ScreenSpaceMask;
			m_ShadowDataList.Add(item);
		}
		m_ShadowMatricesBuffer.SetData(m_ShadowMatrices);
		m_ShadowDataBuffer.SetData(m_ShadowDataList);
	}

	private void ProcessRequests(ref RenderingData renderingData)
	{
		ReleaseEntries();
		ShadowingData shadowData = renderingData.ShadowData;
		m_Packer.Init(shadowData.AtlasSize, shadowData.AtlasSize, rotations: false);
		m_ShadowMatrices.Clear();
		NativeArray<VisibleLight> visibleLights = renderingData.LightData.VisibleLights;
		for (int i = 0; i < visibleLights.Length; i++)
		{
			if (renderingData.CullResults.GetShadowCasterBounds(i, out var _))
			{
				VisibleLight light = visibleLights[i];
				ProcessLight(light, i, ref renderingData);
			}
		}
	}

	private void ProcessLight(VisibleLight light, int lightIndex, ref RenderingData renderingData)
	{
		ShadowingData shadowData = renderingData.ShadowData;
		Camera camera = renderingData.CameraData.Camera;
		if (m_Entries.Count >= 64)
		{
			Debug.LogWarning("The requested shadows do not fit into shadow entries array. " + light.light.name);
		}
		else
		{
			if (light.light == null || light.light.shadows == LightShadows.None)
			{
				return;
			}
			switch (light.lightType)
			{
			case LightType.Spot:
			{
				int num3 = CalculatePunctualResolution(camera, ref light, shadowData.SpotLightResolution);
				Rect rect2 = m_Packer.Insert(num3, num3, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit);
				if (rect2 == m_NullRect)
				{
					Debug.LogWarning("The requested shadows do not fit into shadowmap. " + light.light.name);
					break;
				}
				if (m_ShadowMatrices.Count + 1 > 256)
				{
					Debug.LogWarning("The requested shadows do not fit into shadow matrix array. " + light.light.name);
					break;
				}
				ShadowmapEntry shadowmapEntry2 = ShadowmapEntry.Get(in light, in renderingData);
				shadowmapEntry2.ShadowFlags = ShadowFlags.None;
				if (shadowmapEntry2.Shadows == LightShadows.Soft)
				{
					shadowmapEntry2.ShadowFlags |= ShadowFlags.SoftShadows;
				}
				shadowmapEntry2.LightIndex = lightIndex;
				shadowmapEntry2.Viewport = rect2;
				float num4 = shadowData.AtlasSize;
				shadowmapEntry2.ScaleOffset = new Vector4(rect2.width / num4, rect2.height / num4, rect2.x / num4, rect2.y / num4);
				ShadowSplitData shadowSplitData2 = default(ShadowSplitData);
				shadowSplitData2.cullingSphere.Set(0f, 0f, 0f, float.NegativeInfinity);
				shadowSplitData2.cullingPlaneCount = 0;
				shadowmapEntry2.Splits[0] = shadowSplitData2;
				Matrix4x4 identity = Matrix4x4.identity;
				identity.m22 = -1f;
				Matrix4x4 matrix4x = identity * shadowmapEntry2.LocalToWorldMatrix.inverse;
				float range = light.range;
				float shadowNearPlane = light.light.shadowNearPlane;
				float normalBiasMax = 4f;
				float num5 = CalcGuardAnglePerspective(light.spotAngle, shadowmapEntry2.Viewport.width, GetFilterWidth(shadowmapEntry2), normalBiasMax, 180f - light.spotAngle);
				Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(light.spotAngle + num5, 1f, shadowNearPlane, range), renderIntoTexture: true);
				int count = m_ShadowMatrices.Count;
				ShadowMatrix item = default(ShadowMatrix);
				item.worldToShadow = gPUProjectionMatrix * matrix4x;
				item.lightDirection = -shadowmapEntry2.LocalToWorldMatrix.GetColumn(2);
				float frustumSize = Mathf.Tan(light.spotAngle * 0.5f * (MathF.PI / 180f)) * light.range;
				CalculateShadowBias(shadowmapEntry2, frustumSize, out item.depthBias, out item.normalBias);
				m_ShadowMatrices.Add(item);
				shadowmapEntry2.MatrixIndices[0] = count;
				m_Entries.Add(shadowmapEntry2);
				break;
			}
			case LightType.Directional:
			{
				Vector2 vector = shadowData.DirectionalLightCascades.Count switch
				{
					2 => new Vector2(shadowData.DirectionalLightCascadeResolution * 2, shadowData.DirectionalLightCascadeResolution), 
					3 => new Vector2(shadowData.DirectionalLightCascadeResolution * 3, shadowData.DirectionalLightCascadeResolution), 
					4 => new Vector2(shadowData.DirectionalLightCascadeResolution * 2, shadowData.DirectionalLightCascadeResolution * 2), 
					_ => new Vector2(shadowData.DirectionalLightCascadeResolution, shadowData.DirectionalLightCascadeResolution), 
				};
				Rect rect3 = m_Packer.Insert((int)vector.x, (int)vector.y, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit);
				if (rect3 == m_NullRect)
				{
					Debug.LogWarning("The requested shadows do not fit into shadowmap. " + light.light.name);
					break;
				}
				if (m_ShadowMatrices.Count + shadowData.DirectionalLightCascades.Count > 256)
				{
					Debug.LogWarning("The requested shadows do not fit into shadow matrix array. " + light.light.name);
					break;
				}
				ShadowmapEntry shadowmapEntry3 = ShadowmapEntry.Get(in light, in renderingData);
				shadowmapEntry3.ShadowFlags = ShadowFlags.None;
				if (shadowmapEntry3.Shadows == LightShadows.Soft)
				{
					shadowmapEntry3.ShadowFlags |= ShadowFlags.SoftShadows;
				}
				shadowmapEntry3.LightIndex = lightIndex;
				shadowmapEntry3.Viewport = rect3;
				float num6 = shadowData.AtlasSize;
				shadowmapEntry3.ScaleOffset = new Vector4(rect3.width / num6, rect3.height / num6, rect3.x / num6, rect3.y / num6);
				CaclulateDirectionalLightMatrices(shadowmapEntry3, ref renderingData);
				m_Entries.Add(shadowmapEntry3);
				break;
			}
			case LightType.Point:
			{
				int num = CalculatePunctualResolution(camera, ref light, shadowData.PointLightResolution);
				Rect rect = m_Packer.Insert(num, num, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit);
				if (rect == m_NullRect)
				{
					Debug.LogWarning("The requested shadows do not fit into shadowmap. " + light.light.name);
					break;
				}
				if (m_ShadowMatrices.Count + 4 > 256)
				{
					Debug.LogWarning("The requested shadows do not fit into shadow matrix array. " + light.light.name);
					break;
				}
				ShadowmapEntry shadowmapEntry = ShadowmapEntry.Get(in light, in renderingData);
				shadowmapEntry.ShadowFlags = ShadowFlags.Point;
				if (shadowmapEntry.Shadows == LightShadows.Soft)
				{
					shadowmapEntry.ShadowFlags |= ShadowFlags.SoftShadows;
				}
				shadowmapEntry.LightIndex = lightIndex;
				shadowmapEntry.Viewport = rect;
				float num2 = shadowData.AtlasSize;
				shadowmapEntry.ScaleOffset = new Vector4(rect.width / num2, rect.height / num2, rect.x / num2, rect.y / num2);
				ShadowSplitData shadowSplitData = default(ShadowSplitData);
				Vector4 column = light.localToWorldMatrix.GetColumn(3);
				column.w = light.range;
				shadowSplitData.cullingSphere = column;
				shadowmapEntry.Splits[0] = shadowSplitData;
				CalculatePointLightMatrices(shadowmapEntry);
				m_Entries.Add(shadowmapEntry);
				break;
			}
			}
		}
	}

	private void CalculateShadowBias(ShadowmapEntry entry, float frustumSize, out float depthBias, out float normalBias)
	{
		float num = frustumSize / entry.Viewport.width;
		depthBias = (0f - entry.DepthBias) * num;
		normalBias = (0f - entry.NormalBias) * num;
		if ((entry.ShadowFlags & ShadowFlags.SoftShadows) != 0)
		{
			float num2 = GetFilterWidth(entry) * 0.5f;
			depthBias *= num2;
			normalBias *= num2;
		}
	}

	private void CalculatePointLightMatrices(ShadowmapEntry entry)
	{
		float filterWidth = GetFilterWidth(entry);
		float normalBiasMax = 0.5f;
		float num = CalcGuardAnglePerspective(143.9857f, entry.Viewport.width, filterWidth, normalBiasMax, 36.014297f);
		float num2 = CalcGuardAnglePerspective(125.26439f, entry.Viewport.width, filterWidth, normalBiasMax, 54.73561f);
		m_PointShadowProjMatrices[0] = CreatePerspective(new Vector2(143.9857f + num, 125.26439f + num2), entry.ShadowNearPlane * 0.5f, entry.Range);
		m_PointShadowProjMatrices[1] = CreatePerspective(new Vector2(125.26439f + num2, 143.9857f + num), entry.ShadowNearPlane * 0.5f, entry.Range);
		Vector3 vector = entry.LocalToWorldMatrix.GetColumn(3);
		float4x4 b = float4x4.Translate(-vector);
		for (int i = 0; i < 4; i++)
		{
			float4x4 b2 = math.mul(m_PointScaleMatrices[i], math.mul(m_TetrahedronMatrices[i], b));
			int num3 = i & 1;
			float4x4 a = m_PointShadowProjMatrices[num3];
			ShadowMatrix item = default(ShadowMatrix);
			item.worldToShadow = math.mul(m_PointLightTexMatrices[i], math.mul(a, b2));
			ShadowSplitData shadowSplitData = default(ShadowSplitData);
			TetrahedronUtils.GetFacePlanes(i, vector, ref m_PointSplitPlanes);
			shadowSplitData.cullingPlaneCount = 3;
			for (int j = 0; j < 3; j++)
			{
				shadowSplitData.SetCullingPlane(j, m_PointSplitPlanes[j]);
			}
			entry.Splits[i] = shadowSplitData;
			float frustumSize = Mathf.Tan(1.2565123f) * entry.Range;
			CalculateShadowBias(entry, frustumSize, out item.depthBias, out item.normalBias);
			item.depthBias = 0f;
			item.lightDirection = -TetrahedronUtils.FaceVectors[i];
			entry.MatrixIndices[i] = m_ShadowMatrices.Count;
			m_ShadowMatrices.Add(item);
		}
	}

	private void CaclulateDirectionalLightMatrices(ShadowmapEntry entry, ref RenderingData renderingData)
	{
		ShadowingData shadowData = renderingData.ShadowData;
		ShadowSplitData shadowSplitData = default(ShadowSplitData);
		shadowSplitData.cullingPlaneCount = 0;
		shadowSplitData.cullingSphere.Set(0f, 0f, 0f, float.NegativeInfinity);
		Vector3 vector = new Vector3(1f, 0f, 0f);
		int count = shadowData.DirectionalLightCascades.Count;
		vector = shadowData.DirectionalLightCascades.GetRatios();
		Matrix4x4[] directionalTexMatrices = GetDirectionalTexMatrices(count);
		for (int i = 0; i < count; i++)
		{
			shadowSplitData.cullingPlaneCount = 0;
			shadowSplitData.cullingSphere.Set(0f, 0f, 0f, float.NegativeInfinity);
			renderingData.CullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(entry.LightIndex, i, count, vector, shadowData.DirectionalLightCascadeResolution, shadowData.ShadowNearPlane, out var viewMatrix, out var projMatrix, out shadowSplitData);
			entry.Splits[i] = shadowSplitData;
			projMatrix = GL.GetGPUProjectionMatrix(projMatrix, renderIntoTexture: true);
			ShadowMatrix item = default(ShadowMatrix);
			item.worldToShadow = directionalTexMatrices[i] * projMatrix * viewMatrix;
			item.lightDirection = -entry.LocalToWorldMatrix.GetColumn(2);
			float frustumSize = 2f / projMatrix.m00;
			CalculateShadowBias(entry, frustumSize, out item.depthBias, out item.normalBias);
			_ = shadowSplitData.cullingSphere;
			item.spherePosition = shadowSplitData.cullingSphere;
			item.sphereRadius = shadowSplitData.cullingSphere.w;
			item.sphereRadiusSq = item.sphereRadius * item.sphereRadius;
			entry.MatrixIndices[i] = m_ShadowMatrices.Count;
			m_ShadowMatrices.Add(item);
		}
	}

	private Matrix4x4[] GetDirectionalTexMatrices(int cascadeCount)
	{
		return cascadeCount switch
		{
			2 => m_DirectionalLightTexMatrices2Cascades, 
			3 => m_DirectionalLightTexMatrices3Cascades, 
			4 => m_DirectionalLightTexMatrices4Cascades, 
			_ => m_DirectionalLightTexMatrices1Cascades, 
		};
	}

	private int CalculatePunctualResolution(Camera camera, ref VisibleLight light, int maxResolution)
	{
		float num = (int)Mathf.Max((float)camera.pixelWidth * light.screenRect.width, (float)camera.pixelHeight * light.screenRect.height);
		if (light.lightType == LightType.Point)
		{
			num *= 4f;
		}
		if (num > (float)maxResolution)
		{
			return maxResolution;
		}
		return (int)Mathf.Max((int)((float)(int)(num / 128f) * 128f), 128f);
	}

	public static float CalcGuardAnglePerspective(float angleInDeg, float resolution, float filterWidth, float normalBiasMax, float guardAngleMaxInDeg)
	{
		float num = angleInDeg * 0.5f * (MathF.PI / 180f);
		float num2 = 2f / resolution;
		float num3 = Mathf.Cos(num) * num2;
		float num4 = Mathf.Atan(normalBiasMax * num3 * 1.4142135f);
		num3 = Mathf.Tan(num + num4) * num2;
		num4 = Mathf.Atan((resolution + Mathf.Ceil(filterWidth)) * num3 * 0.5f) * 2f * 57.29578f - angleInDeg;
		num4 *= 2f;
		if (!(num4 < guardAngleMaxInDeg))
		{
			return guardAngleMaxInDeg;
		}
		return num4;
	}

	private float GetFilterWidth(ShadowmapEntry entry)
	{
		return 5f;
	}

	private void ReleaseEntries()
	{
		foreach (ShadowmapEntry entry in m_Entries)
		{
			ShadowmapEntry.Release(entry);
		}
		m_Entries.Clear();
	}

	private Vector3 CalculateLineEquationCoeffs(Vector2 p1, Vector2 p2)
	{
		return new Vector3(p1.y - p2.y, p2.x - p1.x, p1.x * p2.y - p2.x * p1.y);
	}

	private void CalculateTetrahedronMatrices()
	{
		Matrix4x4 matrix4x = CreateRotationY(180f);
		Matrix4x4 matrix4x2 = CreateRotationX(27.367805f);
		m_TetrahedronMatrices[0] = matrix4x2 * matrix4x;
		matrix4x = CreateRotationY(0f);
		matrix4x2 = CreateRotationX(27.367805f);
		Matrix4x4 matrix4x3 = CreateRotationZ(90f);
		m_TetrahedronMatrices[1] = matrix4x3 * matrix4x2 * matrix4x;
		matrix4x = CreateRotationY(270f);
		matrix4x2 = CreateRotationX(-27.367805f);
		m_TetrahedronMatrices[2] = matrix4x2 * matrix4x;
		matrix4x = CreateRotationY(90f);
		matrix4x2 = CreateRotationX(-27.367805f);
		matrix4x3 = CreateRotationZ(90f);
		m_TetrahedronMatrices[3] = matrix4x3 * matrix4x2 * matrix4x;
	}

	private Matrix4x4 CreatePerspective(Vector2 fov, float n, float f)
	{
		Matrix4x4 proj = Matrix4x4.Perspective(fov.y, 1f, n, f);
		proj.m00 = 1f / Mathf.Tan(MathF.PI / 180f * fov.x * 0.5f);
		proj.m11 = 1f / Mathf.Tan(MathF.PI / 180f * fov.y * 0.5f);
		return GL.GetGPUProjectionMatrix(proj, renderIntoTexture: true);
	}

	private Matrix4x4 CreateRotationX(float angle)
	{
		return Matrix4x4.Rotate(Quaternion.Euler(angle, 0f, 0f));
	}

	private Matrix4x4 CreateRotationY(float angle)
	{
		return Matrix4x4.Rotate(Quaternion.Euler(0f, angle, 0f));
	}

	private Matrix4x4 CreateRotationZ(float angle)
	{
		return Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, angle));
	}

	public Vector4[] GetDirectionalLightClips(int cascadeCount)
	{
		return cascadeCount switch
		{
			3 => m_DirectionalLightClips3Cascades, 
			4 => m_DirectionalLightClips4Cascades, 
			_ => m_DirectionalLightClips2Cascades, 
		};
	}
}
