using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

internal sealed class RendererDataRegistry : IDisposable
{
	public readonly struct RendererData
	{
		public readonly Renderer renderer;

		public readonly IRendererProxy proxy;

		public RendererData(Renderer renderer, IRendererProxy proxy)
		{
			this.renderer = renderer;
			this.proxy = proxy;
		}
	}

	private struct ProviderData
	{
		public IOcclusionGeometryProvider provider;

		public int rendererStartIndex;

		public int renderersCount;
	}

	private const int kInvalidIndex = -1;

	private readonly List<ProviderData> m_ProviderDataList;

	private RendererData[] m_RendererArray;

	private NativeArray<float> m_RendererOpacityArray;

	private NativeArray<uint> m_RendererDirtyFlagsArray;

	private NativeArray<float> m_RendererOccludeTimestampArray;

	private NativeArray<uint> m_RendererIndicesArray;

	private NativeArray<OccluderGeometry> m_OccluderGeometryArray;

	private ABox m_SceneBounds;

	private bool m_Changed;

	private bool m_Disposed;

	public RendererData[] RendererArray
	{
		get
		{
			ThrowIfDisposed();
			return m_RendererArray;
		}
	}

	public NativeArray<float> RendererOpacityArray
	{
		get
		{
			ThrowIfDisposed();
			return m_RendererOpacityArray;
		}
	}

	public NativeArray<float> RendererOccludeTimestampArray
	{
		get
		{
			ThrowIfDisposed();
			return m_RendererOccludeTimestampArray;
		}
	}

	public NativeArray<OccluderGeometry> OccluderGeometryArray
	{
		get
		{
			ThrowIfDisposed();
			return m_OccluderGeometryArray;
		}
	}

	public NativeArray<uint> RendererDirtyFlagsArray => m_RendererDirtyFlagsArray;

	public NativeArray<uint> RendererIndicesArray => m_RendererIndicesArray;

	public ABox SceneBounds => m_SceneBounds;

	public bool Changed => m_Changed;

	public RendererDataRegistry()
	{
		m_ProviderDataList = new List<ProviderData>(2);
		m_RendererArray = Array.Empty<RendererData>();
		m_RendererOpacityArray = new NativeArray<float>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_RendererDirtyFlagsArray = new NativeArray<uint>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_RendererOccludeTimestampArray = new NativeArray<float>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_RendererIndicesArray = new NativeArray<uint>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_OccluderGeometryArray = new NativeArray<OccluderGeometry>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_Changed = false;
	}

	public void Dispose()
	{
		if (!m_Disposed)
		{
			m_RendererOpacityArray.Dispose();
			m_RendererDirtyFlagsArray.Dispose();
			m_RendererOccludeTimestampArray.Dispose();
			m_RendererIndicesArray.Dispose();
			m_OccluderGeometryArray.Dispose();
			m_Disposed = true;
		}
	}

	public void AddProvider(IOcclusionGeometryProvider provider)
	{
		ThrowIfDisposed();
		ProviderData providerData = default(ProviderData);
		providerData.provider = provider;
		providerData.rendererStartIndex = -1;
		providerData.renderersCount = 0;
		ProviderData item = providerData;
		m_ProviderDataList.Add(item);
		m_Changed = true;
	}

	public void RemoveProvider(IOcclusionGeometryProvider provider)
	{
		ThrowIfDisposed();
		int i = 0;
		for (int count = m_ProviderDataList.Count; i < count; i++)
		{
			if (m_ProviderDataList[i].provider == provider)
			{
				m_ProviderDataList.RemoveAt(i);
				m_Changed = true;
				break;
			}
		}
	}

	public void Update()
	{
		ThrowIfDisposed();
		if (m_Changed)
		{
			m_Changed = false;
			Refresh();
		}
	}

	private void Refresh()
	{
		MigrateRendererData(out var migratedProvidersCount, out var migratedRenderersCount);
		PopulateRendererData(migratedProvidersCount, migratedRenderersCount);
		PopulateBoundsData();
	}

	private void MigrateRendererData(out int migratedProvidersCount, out int migratedRenderersCount)
	{
		int item = GetProvidersDataCounts().renderersCount;
		if (item != m_RendererArray.Length)
		{
			RendererData[] rendererArray = m_RendererArray;
			NativeArray<float> rendererOpacityArray = m_RendererOpacityArray;
			NativeArray<uint> rendererDirtyFlagsArray = m_RendererDirtyFlagsArray;
			NativeArray<float> rendererOccludeTimestampArray = m_RendererOccludeTimestampArray;
			m_RendererArray = new RendererData[item];
			m_RendererOpacityArray = new NativeArray<float>(item, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_RendererDirtyFlagsArray = new NativeArray<uint>(item, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_RendererOccludeTimestampArray = new NativeArray<float>(item, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			MigrateRendererData(m_ProviderDataList, rendererArray, m_RendererArray, rendererOpacityArray, m_RendererOpacityArray, rendererDirtyFlagsArray, m_RendererDirtyFlagsArray, rendererOccludeTimestampArray, m_RendererOccludeTimestampArray, out migratedProvidersCount, out migratedRenderersCount);
			rendererOpacityArray.Dispose();
			rendererDirtyFlagsArray.Dispose();
			rendererOccludeTimestampArray.Dispose();
		}
		else
		{
			MigrateRendererData(m_ProviderDataList, m_RendererArray, m_RendererArray, m_RendererOpacityArray, m_RendererOpacityArray, m_RendererDirtyFlagsArray, m_RendererDirtyFlagsArray, m_RendererOccludeTimestampArray, m_RendererOccludeTimestampArray, out migratedProvidersCount, out migratedRenderersCount);
		}
	}

	private static void MigrateRendererData(List<ProviderData> providerDataList, RendererData[] srcRendererArray, RendererData[] dstRendererArray, NativeArray<float> srcRendererOpacityArray, NativeArray<float> dstRendererOpacityArray, NativeArray<uint> srcRendererDirtyFlagsArray, NativeArray<uint> dstRendererDirtyFlagsArray, NativeArray<float> srcRendererTimestampArray, NativeArray<float> dstRendererTimestampArray, out int migratedProvidersCount, out int migratedRenderersCount)
	{
		int num = 0;
		int i = 0;
		for (int count = providerDataList.Count; i < count; i++)
		{
			ProviderData value = providerDataList[i];
			if (value.rendererStartIndex < 0)
			{
				break;
			}
			Array.Copy(srcRendererArray, value.rendererStartIndex, dstRendererArray, num, value.renderersCount);
			NativeArray<float>.Copy(srcRendererOpacityArray, value.rendererStartIndex, dstRendererOpacityArray, num, value.renderersCount);
			NativeArray<uint>.Copy(srcRendererDirtyFlagsArray, value.rendererStartIndex, dstRendererDirtyFlagsArray, num, value.renderersCount);
			NativeArray<float>.Copy(srcRendererTimestampArray, value.rendererStartIndex, dstRendererTimestampArray, num, value.renderersCount);
			value.rendererStartIndex = num;
			providerDataList[i] = value;
			num += value.renderersCount;
		}
		migratedProvidersCount = i;
		migratedRenderersCount = num;
	}

	private void PopulateRendererData(int providerListOffset, int rendererArrayOffset)
	{
		int i = providerListOffset;
		for (int count = m_ProviderDataList.Count; i < count; i++)
		{
			ProviderData value = m_ProviderDataList[i];
			OcclusionGeometry occlusionGeometry = value.provider.OcclusionGeometry;
			int num = occlusionGeometry.renderers.Length;
			value.rendererStartIndex = rendererArrayOffset;
			value.renderersCount = num;
			m_ProviderDataList[i] = value;
			int num2 = 0;
			int num3 = rendererArrayOffset;
			while (num2 < num)
			{
				Renderer renderer = occlusionGeometry.renderers[num2];
				IRendererProxy component = renderer.GetComponent<IRendererProxy>();
				m_RendererArray[num3] = new RendererData(renderer, component);
				num2++;
				num3++;
			}
			int num4 = rendererArrayOffset + num;
			while (rendererArrayOffset < num4)
			{
				m_RendererOpacityArray[rendererArrayOffset] = 1f;
				m_RendererDirtyFlagsArray[rendererArrayOffset] = 0u;
				m_RendererOccludeTimestampArray[rendererArrayOffset] = float.MinValue;
				rendererArrayOffset++;
			}
		}
	}

	private void PopulateBoundsData()
	{
		(int renderersCount, int boundsCount, int indicesCount) providersDataCounts = GetProvidersDataCounts();
		int item = providersDataCounts.boundsCount;
		int item2 = providersDataCounts.indicesCount;
		if (item != m_OccluderGeometryArray.Length)
		{
			m_OccluderGeometryArray.Dispose();
			m_OccluderGeometryArray = new NativeArray<OccluderGeometry>(item, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (item2 != m_RendererIndicesArray.Length)
		{
			m_RendererIndicesArray.Dispose();
			m_RendererIndicesArray = new NativeArray<uint>(item2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (item == 0)
		{
			m_SceneBounds = default(ABox);
			return;
		}
		float3 min = new float3(float.MaxValue);
		float3 max = new float3(float.MinValue);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		ABox aBox = default(ABox);
		OBox oBox = default(OBox);
		foreach (ProviderData providerData in m_ProviderDataList)
		{
			OcclusionGeometry occlusionGeometry = providerData.provider.OcclusionGeometry;
			int num4 = num2;
			int num5 = num;
			int num6 = occlusionGeometry.indices.Length;
			int num7 = occlusionGeometry.bounds.Length;
			int num8 = occlusionGeometry.renderers.Length;
			for (int i = 0; i < num6; i++)
			{
				m_RendererIndicesArray[num4++] = (uint)(num3 + occlusionGeometry.indices[i]);
			}
			for (int j = 0; j < num7; j++)
			{
				ref OcclusionGeometry.PackedBounds reference = ref occlusionGeometry.bounds[j];
				reference.Unpack(ref aBox, ref oBox);
				min = math.min(min, aBox.min);
				max = math.max(max, aBox.max);
				m_OccluderGeometryArray[num5++] = new OccluderGeometry(num2 + reference.ib, num2 + reference.ie, aBox, oBox);
			}
			num2 += num6;
			num += num7;
			num3 += num8;
		}
		m_SceneBounds = new ABox(in min, in max);
	}

	private (int renderersCount, int boundsCount, int indicesCount) GetProvidersDataCounts()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (ProviderData providerData in m_ProviderDataList)
		{
			OcclusionGeometry occlusionGeometry = providerData.provider.OcclusionGeometry;
			num += occlusionGeometry.renderers.Length;
			num2 += occlusionGeometry.bounds.Length;
			num3 += occlusionGeometry.indices.Length;
		}
		return (renderersCount: num, boundsCount: num2, indicesCount: num3);
	}

	private void ThrowIfDisposed()
	{
		if (m_Disposed)
		{
			throw new ObjectDisposedException("RendererDataRegistry");
		}
	}

	private bool IsProviderRegistered(IOcclusionGeometryProvider provider)
	{
		int i = 0;
		for (int count = m_ProviderDataList.Count; i < count; i++)
		{
			if (m_ProviderDataList[i].provider == provider)
			{
				return true;
			}
		}
		return false;
	}

	[Conditional("UNITY_ASSERTIONS")]
	private void AssertClassInvariant()
	{
		if (!m_Changed)
		{
			GetProvidersDataCounts();
		}
	}
}
