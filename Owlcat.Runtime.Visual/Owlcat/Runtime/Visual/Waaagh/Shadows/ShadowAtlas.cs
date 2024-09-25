using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

internal sealed class ShadowAtlas : IDisposable
{
	private readonly string m_Name;

	private ShadowResolution m_Resolution;

	private ShadowAtlasAllocator m_Allocator;

	private RTHandle m_Texture;

	public ShadowResolution Resolution
	{
		get
		{
			return m_Resolution;
		}
		set
		{
			if (m_Resolution != value)
			{
				m_Resolution = value;
				Cleanup();
				Setup();
			}
		}
	}

	public ShadowAtlasAllocator Allocator => m_Allocator;

	public RTHandle Texture => m_Texture;

	public ShadowAtlas(string name, ShadowResolution resolution)
	{
		m_Name = name;
		m_Resolution = resolution;
		Setup();
	}

	public void Dispose()
	{
		m_Allocator.Dispose();
		RTHandles.Release(m_Texture);
	}

	private void Cleanup()
	{
		m_Allocator.Dispose();
		RTHandles.Release(m_Texture);
	}

	private void Setup()
	{
		m_Allocator = new ShadowAtlasAllocator(m_Resolution, Unity.Collections.Allocator.Persistent);
		m_Texture = RTHandles.Alloc((int)m_Resolution, (int)m_Resolution, 1, DepthBits.Depth16, GraphicsFormat.D16_UNorm, FilterMode.Bilinear, TextureWrapMode.Clamp, TextureDimension.Tex2D, enableRandomWrite: false, useMipMap: false, autoGenerateMips: true, isShadowMap: true, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, RenderTextureMemoryless.None, VRTextureUsage.None, m_Name);
	}
}
