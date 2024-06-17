using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Pathfinding;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using RogueTrader.Code.ShaderConsts;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.LocalMap;

public class WarhammerLocalMapRenderer : MonoBehaviour
{
	public struct DrawResults
	{
		public RenderTexture ColorRT;

		public Rect ScreenRect;

		public Vector4 LocalMapFowScaleOffset;
	}

	public enum Downsample
	{
		None = 1,
		Half = 2,
		Quarter = 4
	}

	public enum BlurDirections
	{
		Diagonal,
		Straight,
		All
	}

	public static readonly string STRAIGHT_DIRECTIONS = "STRAIGHT_DIRECTIONS";

	public static readonly string ALL_DIRECTIONS = "ALL_DIRECTIONS";

	public static int _HighlightingBlurOffset = Shader.PropertyToID("_HighlightingBlurOffset");

	public static int _LocalMapMainColor = Shader.PropertyToID("_LocalMapMainColor");

	public static int _LocalMapBorderColor = Shader.PropertyToID("_LocalMapBorderColor");

	public static int _LocalMapMask = Shader.PropertyToID("_LocalMapMask");

	private CustomGridGraph m_CachedGraph;

	private BlueprintAreaPart m_CachedArea;

	private Texture2D m_LocalMapTexture;

	private bool m_IsGraphDirty;

	private bool m_IsMapTextureDirty;

	private RenderTexture m_ColorRT;

	private Bounds m_GraphBounds;

	public Material LocalMapBakerMaterial;

	public Downsample DownsampleFactor = Downsample.Half;

	public BlurDirections BlurDirectionMode = BlurDirections.All;

	[Range(0f, 50f)]
	public int BlurIterations = 2;

	[Range(0f, 3f)]
	public float BlurMinSpread = 0.65f;

	[Range(0f, 3f)]
	public float BlurSpread = 0.25f;

	public Color MainColor = new Color(0.1f, 1f, 0.1f, 0.3f);

	public Color BorderColor = new Color(0.4f, 1f, 0.4f, 0.5f);

	public static WarhammerLocalMapRenderer Instance { get; private set; }

	private void OnEnable()
	{
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnGraphsUpdated));
		m_IsGraphDirty = true;
		m_IsMapTextureDirty = true;
		Instance = this;
	}

	private void OnDisable()
	{
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnGraphsUpdated));
	}

	private void OnDestroy()
	{
		if (m_ColorRT != null)
		{
			m_ColorRT.Release();
			m_ColorRT = null;
		}
		if (m_LocalMapTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(m_LocalMapTexture);
			m_LocalMapTexture = null;
		}
	}

	private void OnGraphsUpdated(AstarPath script)
	{
		m_IsGraphDirty = true;
	}

	private void Update()
	{
		CustomGridGraph graph = GetGraph(AstarPath.active);
		if (graph != m_CachedGraph)
		{
			m_CachedGraph = graph;
			m_IsGraphDirty = true;
		}
		BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		if (m_CachedArea != currentlyLoadedAreaPart)
		{
			m_CachedArea = currentlyLoadedAreaPart;
			m_IsMapTextureDirty = true;
		}
	}

	public DrawResults Draw()
	{
		if (m_IsGraphDirty)
		{
			UpdateGraphTexture();
			m_IsGraphDirty = false;
			m_IsMapTextureDirty = true;
		}
		if (m_IsMapTextureDirty)
		{
			UpdateMapTexture();
			m_IsMapTextureDirty = false;
		}
		return GenerateDrawResults();
	}

	private void UpdateGraphTexture()
	{
		int num = 4;
		List<int>[] array = new List<int>[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Owlcat.Runtime.Core.Utility.ListPool<int>.Claim();
			array[i].Clear();
		}
		List<float> list = Owlcat.Runtime.Core.Utility.ListPool<float>.Claim();
		Color[] array2 = ArrayPool<Color>.Shared.Rent(m_CachedGraph.nodes.Length);
		float3 @float = m_CachedGraph.nodes[0].Vector3Position;
		float3 float2 = m_CachedGraph.nodes[0].Vector3Position;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		for (int j = 0; j < m_CachedGraph.nodes.Length; j++)
		{
			CustomGridNode obj = m_CachedGraph.nodes[j];
			Vector3 vector3Position = obj.Vector3Position;
			@float = math.min(@float, vector3Position);
			float2 = math.max(float2, vector3Position);
			if (obj.Walkable)
			{
				num2 = math.min(vector3Position.y, num2);
				num3 = math.max(vector3Position.y, num3);
			}
			list.Add(vector3Position.y);
		}
		float x = num3 - num2;
		x = math.rcp(x);
		for (int k = 0; k < list.Count; k++)
		{
			list[k] = (list[k] - num2) * x;
			int x2 = (int)math.round(list[k] * (float)(num - 1));
			x2 = math.clamp(x2, 0, num - 1);
			array[x2].Add(k);
		}
		for (int l = 0; l < num; l++)
		{
			List<int> list2 = array[l];
			float num4 = ((float)l + 1f) / (float)num;
			for (int m = 0; m < list2.Count; m++)
			{
				int num5 = list2[m];
				if (m_CachedGraph.nodes[num5].Walkable)
				{
					array2[num5] = new Color(num4, num4, num4, num4);
				}
				else
				{
					array2[num5] = new Color(0f, 0f, 0f, 0f);
				}
			}
		}
		m_GraphBounds = default(Bounds);
		m_GraphBounds.min = @float;
		m_GraphBounds.max = float2;
		if (m_LocalMapTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(m_LocalMapTexture);
		}
		m_LocalMapTexture = new Texture2D(m_CachedGraph.width, m_CachedGraph.depth, TextureFormat.R8, mipChain: false);
		m_LocalMapTexture.filterMode = FilterMode.Point;
		m_LocalMapTexture.name = "LocalMap_NavMeshTexture";
		m_LocalMapTexture.wrapMode = TextureWrapMode.Clamp;
		m_LocalMapTexture.SetPixels(array2);
		m_LocalMapTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		ArrayPool<Color>.Shared.Return(array2);
		for (int n = 0; n < num; n++)
		{
			Owlcat.Runtime.Core.Utility.ListPool<int>.Release(array[n]);
		}
		Owlcat.Runtime.Core.Utility.ListPool<float>.Release(list);
	}

	private void UpdateMapTexture()
	{
		if (m_ColorRT != null)
		{
			m_ColorRT.Release();
			m_ColorRT = null;
		}
		if (m_CachedArea == null)
		{
			return;
		}
		m_ColorRT = new RenderTexture((int)m_CachedArea.Bounds.LocalMapBounds.size.x * 5, (int)m_CachedArea.Bounds.LocalMapBounds.size.z * 5, 0, RenderTextureFormat.ARGB32);
		m_ColorRT.name = "LocalMap_ColorRT";
		float3 @float = m_CachedArea.Bounds.LocalMapBounds.min;
		_ = (float3)m_CachedArea.Bounds.LocalMapBounds.max;
		float3 float2 = m_CachedArea.Bounds.LocalMapBounds.size;
		float3 float3 = m_GraphBounds.min;
		_ = (float3)m_GraphBounds.max;
		float3 float4 = m_GraphBounds.size;
		float2 xz = (float2 / float4).xz;
		float2 float5 = (@float.xz - float3.xz) / float4.xz;
		LocalMapBakerMaterial.SetVector(ShaderProps._ScaleOffset, new Vector4(xz.x, xz.y, float5.x, float5.y));
		RenderTexture temporary = RenderTexture.GetTemporary(new RenderTextureDescriptor(m_ColorRT.width, m_ColorRT.height, RenderTextureFormat.R8, 0));
		Graphics.Blit(m_LocalMapTexture, temporary, LocalMapBakerMaterial, 0);
		int num = Mathf.Max(1, (int)DownsampleFactor);
		RenderTextureDescriptor desc = new RenderTextureDescriptor(m_ColorRT.width / num, m_ColorRT.height / num, RenderTextureFormat.R8, 0);
		RenderTexture temporary2 = RenderTexture.GetTemporary(desc);
		RenderTexture temporary3 = RenderTexture.GetTemporary(desc);
		CoreUtils.SetKeyword(LocalMapBakerMaterial, STRAIGHT_DIRECTIONS, BlurDirectionMode == BlurDirections.Straight);
		CoreUtils.SetKeyword(LocalMapBakerMaterial, ALL_DIRECTIONS, BlurDirectionMode == BlurDirections.All);
		Graphics.Blit(temporary, temporary2);
		bool flag = true;
		for (int i = 0; i < BlurIterations; i++)
		{
			float value = BlurMinSpread + BlurSpread * (float)i;
			Shader.SetGlobalFloat(_HighlightingBlurOffset, value);
			if (flag)
			{
				Graphics.Blit(temporary2, temporary3, LocalMapBakerMaterial, 1);
			}
			else
			{
				Graphics.Blit(temporary3, temporary2, LocalMapBakerMaterial, 1);
			}
			flag = !flag;
		}
		Shader.SetGlobalColor(_LocalMapBorderColor, BorderColor);
		Shader.SetGlobalColor(_LocalMapMainColor, MainColor);
		Shader.SetGlobalTexture(_LocalMapMask, temporary);
		Graphics.Blit(flag ? temporary2 : temporary3, m_ColorRT, LocalMapBakerMaterial, 2);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary3);
	}

	private CustomGridGraph GetGraph(AstarPath active)
	{
		if (!(active != null))
		{
			return null;
		}
		return active.graphs?.OfType<CustomGridGraph>().FirstOrDefault();
	}

	private DrawResults GenerateDrawResults()
	{
		DrawResults result = default(DrawResults);
		result.ColorRT = m_ColorRT;
		result.ScreenRect = CalculateScreenRect(Game.GetCamera());
		result.LocalMapFowScaleOffset = CalculateFowScaleOffset();
		return result;
	}

	private Vector4 CalculateFowScaleOffset()
	{
		float3 @float = m_CachedArea.Bounds.LocalMapBounds.size;
		float3 float2 = m_CachedArea.Bounds.FogOfWarBounds.size;
		float3 float3 = m_CachedArea.Bounds.LocalMapBounds.min;
		float3 float4 = m_CachedArea.Bounds.FogOfWarBounds.min;
		float2 float5 = (float3.xz - float4.xz) / float2.xz;
		float2 float6 = @float.xz / float2.xz;
		return new Vector4(float6.x, float6.y, float5.x, float5.y);
	}

	public Rect CalculateScreenRect(Camera cam)
	{
		if (m_CachedArea == null || cam == null)
		{
			return default(Rect);
		}
		Ray ray = cam.ViewportPointToRay(new Vector3(0f, 0f, 1f));
		Ray ray2 = cam.ViewportPointToRay(new Vector3(1f, 0f, 1f));
		Ray ray3 = cam.ViewportPointToRay(new Vector3(1f, 1f, 1f));
		Ray ray4 = cam.ViewportPointToRay(new Vector3(0f, 1f, 1f));
		CameraRig instance = CameraRig.Instance;
		Plane plane = new Plane(Vector3.up, instance.transform.position);
		plane.Raycast(ray, out var enter);
		plane.Raycast(ray2, out var enter2);
		plane.Raycast(ray3, out var enter3);
		plane.Raycast(ray4, out var enter4);
		float3 x = ray.origin + ray.direction * enter;
		float3 y = ray2.origin + ray2.direction * enter2;
		float3 x2 = ray3.origin + ray3.direction * enter3;
		float3 y2 = ray4.origin + ray4.direction * enter4;
		float3 @float = m_CachedArea.Bounds.LocalMapBounds.min;
		_ = (float3)m_CachedArea.Bounds.LocalMapBounds.max;
		float3 float2 = m_CachedArea.Bounds.LocalMapBounds.size;
		x -= @float;
		y -= @float;
		x2 -= @float;
		y2 -= @float;
		x /= float2;
		y /= float2;
		x2 /= float2;
		y2 /= float2;
		Rect result = default(Rect);
		result.min = math.min(math.min(x, y), math.min(x2, y2)).xz;
		result.max = math.max(math.max(x, y), math.max(x2, y2)).xz;
		return result;
	}

	public Vector3 ViewportToWorldPoint(Vector3 localPos)
	{
		int layerMask = 2359553;
		Bounds localMapBounds = m_CachedArea.Bounds.LocalMapBounds;
		Vector3 origin = new Vector3(Mathf.Lerp(localMapBounds.min.x, localMapBounds.max.x, localPos.x), localMapBounds.max.y, Mathf.Lerp(localMapBounds.min.z, localMapBounds.max.z, localPos.y));
		Ray ray = new Ray(origin, Vector3.down);
		if (Physics.Raycast(ray, out var hitInfo, localMapBounds.size.y, layerMask))
		{
			return hitInfo.point;
		}
		Plane plane = ((m_CachedArea != null) ? new Plane(Vector3.up, localMapBounds.center) : new Plane(Vector3.up, default(Vector3)));
		plane.Raycast(ray, out var enter);
		return ray.origin + ray.direction * enter;
	}

	public Vector3 WorldToViewportPoint(float3 worldPos)
	{
		float3 @float = m_CachedArea.Bounds.LocalMapBounds.min;
		float3 float2 = m_CachedArea.Bounds.LocalMapBounds.size;
		float3 float3 = worldPos - @float;
		float3 /= float2;
		float3.y = 0f;
		return float3.xzy;
	}
}
