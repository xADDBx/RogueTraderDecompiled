using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.GPUSkinning;

public class GPUSkinningSystem
{
	private ComputeBuffer m_SkinningBuffer;

	private bool m_IsDirty;

	private HashSet<GPUSkinningClipData> m_SkinningData = new HashSet<GPUSkinningClipData>();

	public static GPUSkinningSystem Instance { get; private set; }

	static GPUSkinningSystem()
	{
		Instance = new GPUSkinningSystem();
	}

	public void Initialize()
	{
	}

	public void Dispose()
	{
		if (m_SkinningBuffer != null)
		{
			m_SkinningBuffer.Release();
		}
	}

	public void RegisterData(GPUSkinningClipData data)
	{
		if (!m_SkinningData.Contains(data))
		{
			m_IsDirty = true;
			m_SkinningData.Add(data);
		}
	}

	public void RegisterData(IEnumerable<GPUSkinningClipData> data)
	{
		foreach (GPUSkinningClipData datum in data)
		{
			RegisterData(datum);
		}
	}

	public void UnregisterData(GPUSkinningClipData data)
	{
		if (m_SkinningData.Contains(data))
		{
			m_IsDirty = true;
			m_SkinningData.Remove(data);
		}
	}

	public void UnregisterData(IEnumerable<GPUSkinningClipData> data)
	{
		foreach (GPUSkinningClipData datum in data)
		{
			UnregisterData(datum);
		}
	}

	public void Submit()
	{
		if (!m_IsDirty)
		{
			return;
		}
		int num = 0;
		foreach (GPUSkinningClipData skinningDatum in m_SkinningData)
		{
			num += skinningDatum.Frames.Length;
		}
		if (m_SkinningBuffer == null || !m_SkinningBuffer.IsValid() || m_SkinningBuffer.count != num)
		{
			if (m_SkinningBuffer != null)
			{
				m_SkinningBuffer.Release();
			}
			if (num > 0)
			{
				m_SkinningBuffer = new ComputeBuffer(num, Marshal.SizeOf<Matrix4x4>(), ComputeBufferType.Structured);
				m_SkinningBuffer.name = "_GpuSkinningFrames";
			}
		}
		if (m_SkinningBuffer == null)
		{
			return;
		}
		int num2 = 0;
		foreach (GPUSkinningClipData skinningDatum2 in m_SkinningData)
		{
			skinningDatum2.RuntimeBufferOffset = num2;
			m_SkinningBuffer.SetData(skinningDatum2.Frames, 0, num2, skinningDatum2.Frames.Length);
			num2 += skinningDatum2.Frames.Length;
		}
		Shader.SetGlobalBuffer(ComputeBufferId._GpuSkinningFrames, m_SkinningBuffer);
	}
}
