using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public class GBuffer
{
	private const string kProfilerTag = "Initialize GBuffer";

	private const int kMaxDepthPyramidLevelCount = 16;

	private RenderTargetHandle m_CameraColorRt;

	private RenderTargetHandle m_CameraColorPyramidRt;

	private RenderTargetHandle m_CameraDepthRt;

	private RenderTargetHandle m_CameraDepthCopyRt;

	private RenderTargetHandle m_CameraAlbedoRt;

	private RenderTargetHandle m_CameraSpecularRt;

	private RenderTargetHandle m_CameraNormalsRt;

	private RenderTargetHandle m_CameraBakedGIRt;

	private RenderTargetHandle m_CameraShadowmaskRT;

	private RenderTargetHandle m_CameraTranslucencyRT;

	private RenderTargetHandle m_CameraDeferredReflectionsRT;

	private RenderTargetIdentifier[] m_ForwardGBuffer = new RenderTargetIdentifier[3];

	private RenderTargetIdentifier[] m_DeferredGBuffer = new RenderTargetIdentifier[7];

	private RenderPath m_RenderPath;

	private RenderTextureFormat m_CameraColorFormat;

	private Vector4[] m_DepthPyramidMipRects = new Vector4[16];

	private int2 m_DepthPyramidTextureSize = 0;

	private int m_DepthPyramidLodCount;

	private Vector4 m_DepthPyramidSamplingRatio = new Vector4(1f, 1f, 0f, 0f);

	public ref RenderTargetHandle CameraColorRt => ref m_CameraColorRt;

	public ref RenderTargetHandle CameraColorPyramidRt => ref m_CameraColorPyramidRt;

	public ref RenderTargetHandle CameraDepthRt => ref m_CameraDepthRt;

	public ref RenderTargetHandle CameraDepthCopyRt => ref m_CameraDepthCopyRt;

	public ref RenderTargetHandle CameraAlbedoRt => ref m_CameraAlbedoRt;

	public ref RenderTargetHandle CameraSpecularRt => ref m_CameraSpecularRt;

	public ref RenderTargetHandle CameraNormalsRt => ref m_CameraNormalsRt;

	public ref RenderTargetHandle CameraBakedGIRt => ref m_CameraBakedGIRt;

	public ref RenderTargetHandle CameraShadowmaskRT => ref m_CameraShadowmaskRT;

	public ref RenderTargetHandle CameraTranslucencyRT => ref m_CameraTranslucencyRT;

	public ref RenderTargetHandle CameraDeferredReflectionsRT => ref m_CameraDeferredReflectionsRT;

	public RenderTargetIdentifier[] ForwardGBuffer => m_ForwardGBuffer;

	public RenderTargetIdentifier[] DeferredGBuffer => m_DeferredGBuffer;

	public RenderPath RenderPath => m_RenderPath;

	public RenderTextureFormat ColorPyramidFormat
	{
		get
		{
			if (RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float))
			{
				return RenderTextureFormat.RGB111110Float;
			}
			if (RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.ARGB2101010))
			{
				return RenderTextureFormat.ARGB2101010;
			}
			return m_CameraColorFormat;
		}
	}

	public Vector4[] DepthPyramidMipRects => m_DepthPyramidMipRects;

	public int2 DepthPyramidTextureSize => m_DepthPyramidTextureSize;

	public int DepthPyramidLodCount => m_DepthPyramidLodCount;

	public Vector4 DepthPyramidSamplingRatio => m_DepthPyramidSamplingRatio;

	internal GBuffer()
	{
		m_CameraColorRt.Init("_CameraColorRT");
		m_CameraColorPyramidRt.Init("_CameraColorPyramidRT");
		m_CameraDepthRt.Init("_CameraDepthRT");
		m_CameraDepthCopyRt.Init("_CameraDepthCopyRT");
		m_CameraAlbedoRt.Init("_CameraAlbedoRT");
		m_CameraNormalsRt.Init("_CameraNormalsRT");
		m_CameraBakedGIRt.Init("_CameraBakedGIRT");
		m_CameraShadowmaskRT.Init("_CameraShadowmaskRT");
		m_CameraTranslucencyRT.Init("_CameraTranslucencyRT");
		m_CameraDeferredReflectionsRT.Init("_CameraDeferredReflectionsRT");
	}

	public void Initialize(RenderPath renderPath, ScriptableRenderContext context, ref RenderingData renderingData, bool deferredUseCompute)
	{
		if (!renderingData.CameraData.IsFirstInChain)
		{
			return;
		}
		m_RenderPath = renderPath;
		CommandBuffer commandBuffer = CommandBufferPool.Get("Initialize GBuffer");
		CameraData cameraData = renderingData.CameraData;
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.CameraTargetDescriptor;
		cameraTargetDescriptor.depthBufferBits = 24;
		cameraTargetDescriptor.autoGenerateMips = false;
		cameraTargetDescriptor.useMipMap = false;
		cameraTargetDescriptor.colorFormat = RenderTextureFormat.Depth;
		commandBuffer.GetTemporaryRT(m_CameraDepthRt.Id, cameraTargetDescriptor, FilterMode.Point);
		RenderTextureDescriptor cameraTargetDescriptor2 = cameraData.CameraTargetDescriptor;
		m_CameraColorFormat = cameraTargetDescriptor2.colorFormat;
		cameraTargetDescriptor2.enableRandomWrite = renderPath == RenderPath.Deferred && deferredUseCompute;
		cameraTargetDescriptor2.depthBufferBits = 0;
		commandBuffer.GetTemporaryRT(m_CameraColorRt.Id, cameraTargetDescriptor2, FilterMode.Bilinear);
		if (renderingData.CameraData.IsDistortionEnabled || renderPath == RenderPath.Deferred)
		{
			RenderTextureDescriptor desc = cameraTargetDescriptor2;
			desc.useMipMap = true;
			desc.autoGenerateMips = false;
			desc.enableRandomWrite = false;
			if (renderingData.CameraData.IsScreenSpaceReflectionsEnabled)
			{
				desc.colorFormat = m_CameraColorFormat;
			}
			else
			{
				desc.colorFormat = ColorPyramidFormat;
			}
			commandBuffer.GetTemporaryRT(m_CameraColorPyramidRt.Id, desc, FilterMode.Bilinear);
		}
		if (renderingData.CameraData.IsDecalsEnabled || renderingData.CameraData.IsDistortionEnabled || renderingData.CameraData.IsPostProcessEnabled || renderPath == RenderPath.Deferred)
		{
			if (renderingData.CameraData.IsNeedDepthPyramid && renderPath == RenderPath.Deferred)
			{
				ComputePackedMipChainInfo(new int2(cameraTargetDescriptor.width, cameraTargetDescriptor.height));
				cameraTargetDescriptor.width = m_DepthPyramidTextureSize.x;
				cameraTargetDescriptor.height = m_DepthPyramidTextureSize.y;
				cameraTargetDescriptor.enableRandomWrite = true;
			}
			else
			{
				m_DepthPyramidSamplingRatio = new Vector4(1f, 1f, 0f, 0f);
			}
			cameraTargetDescriptor.depthBufferBits = 0;
			cameraTargetDescriptor.colorFormat = RenderTextureFormat.RFloat;
			commandBuffer.GetTemporaryRT(m_CameraDepthCopyRt.Id, cameraTargetDescriptor, FilterMode.Point);
		}
		if (renderPath == RenderPath.Deferred)
		{
			RenderTextureDescriptor cameraTargetDescriptor3 = cameraData.CameraTargetDescriptor;
			cameraTargetDescriptor3.depthBufferBits = 0;
			cameraTargetDescriptor3.colorFormat = RenderTextureFormat.ARGB32;
			commandBuffer.GetTemporaryRT(m_CameraAlbedoRt.Id, cameraTargetDescriptor3, FilterMode.Point);
			RenderTextureDescriptor cameraTargetDescriptor4 = cameraData.CameraTargetDescriptor;
			cameraTargetDescriptor4.depthBufferBits = 0;
			cameraTargetDescriptor4.colorFormat = RenderTextureFormat.ARGB32;
			if (renderPath == RenderPath.Deferred && renderingData.CameraData.IsDecalsEnabled && renderingData.CameraData.IsScreenSpaceReflectionsEnabled)
			{
				cameraTargetDescriptor4.enableRandomWrite = true;
			}
			commandBuffer.GetTemporaryRT(m_CameraSpecularRt.Id, cameraTargetDescriptor4, FilterMode.Point);
			RenderTextureDescriptor cameraTargetDescriptor5 = cameraData.CameraTargetDescriptor;
			cameraTargetDescriptor5.depthBufferBits = 0;
			cameraTargetDescriptor5.sRGB = false;
			cameraTargetDescriptor5.colorFormat = RenderTextureFormat.ARGB32;
			commandBuffer.GetTemporaryRT(m_CameraTranslucencyRT.Id, cameraTargetDescriptor5, FilterMode.Point);
			if (cameraData.IsScreenSpaceReflectionsEnabled)
			{
				RenderTextureDescriptor desc2 = cameraTargetDescriptor2;
				desc2.colorFormat = ColorPyramidFormat;
				desc2.sRGB = false;
				commandBuffer.GetTemporaryRT(m_CameraDeferredReflectionsRT.Id, desc2, FilterMode.Bilinear);
			}
		}
		RenderTextureDescriptor cameraTargetDescriptor6 = cameraData.CameraTargetDescriptor;
		cameraTargetDescriptor6.depthBufferBits = 0;
		cameraTargetDescriptor6.colorFormat = RenderTextureFormat.ARGB32;
		cameraTargetDescriptor6.sRGB = false;
		if (renderPath == RenderPath.Deferred && renderingData.CameraData.IsDecalsEnabled && renderingData.CameraData.IsScreenSpaceReflectionsEnabled)
		{
			cameraTargetDescriptor6.enableRandomWrite = true;
		}
		commandBuffer.GetTemporaryRT(m_CameraNormalsRt.Id, cameraTargetDescriptor6, FilterMode.Bilinear);
		RenderTextureDescriptor cameraTargetDescriptor7 = cameraData.CameraTargetDescriptor;
		cameraTargetDescriptor7.depthBufferBits = 0;
		cameraTargetDescriptor7.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		cameraTargetDescriptor7.sRGB = false;
		commandBuffer.GetTemporaryRT(m_CameraBakedGIRt.Id, cameraTargetDescriptor7, FilterMode.Point);
		RenderTextureDescriptor cameraTargetDescriptor8 = cameraData.CameraTargetDescriptor;
		cameraTargetDescriptor8.sRGB = false;
		cameraTargetDescriptor8.depthBufferBits = 0;
		cameraTargetDescriptor8.colorFormat = RenderTextureFormat.ARGB32;
		commandBuffer.GetTemporaryRT(m_CameraShadowmaskRT.Id, cameraTargetDescriptor8, FilterMode.Point);
		m_ForwardGBuffer[0] = CameraNormalsRt.Identifier();
		m_ForwardGBuffer[1] = m_CameraBakedGIRt.Identifier();
		m_ForwardGBuffer[2] = m_CameraShadowmaskRT.Identifier();
		m_DeferredGBuffer[0] = m_CameraAlbedoRt.Identifier();
		m_DeferredGBuffer[1] = m_CameraSpecularRt.Identifier();
		m_DeferredGBuffer[2] = m_CameraNormalsRt.Identifier();
		m_DeferredGBuffer[3] = m_CameraColorRt.Identifier();
		m_DeferredGBuffer[4] = m_CameraTranslucencyRT.Identifier();
		m_DeferredGBuffer[5] = m_CameraBakedGIRt.Identifier();
		m_DeferredGBuffer[6] = m_CameraShadowmaskRT.Identifier();
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	public void ComputePackedMipChainInfo(int2 viewportSize)
	{
		m_DepthPyramidTextureSize = viewportSize;
		m_DepthPyramidMipRects[0] = new Vector4(0f, 0f, viewportSize.x, viewportSize.y);
		int num = 0;
		int2 @int = viewportSize;
		do
		{
			num++;
			@int.x = Math.Max(1, @int.x + 1 >> 1);
			@int.y = Math.Max(1, @int.y + 1 >> 1);
			float4 @float = m_DepthPyramidMipRects[num - 1];
			int2 int2 = (int2)@float.xy;
			int2 int3 = int2 + (int2)@float.zw;
			int2 int4 = 0;
			if (((uint)num & (true ? 1u : 0u)) != 0)
			{
				int4.x = int2.x;
				int4.y = int3.y;
			}
			else
			{
				int4.x = int3.x;
				int4.y = int2.y;
			}
			m_DepthPyramidMipRects[num] = new Vector4(int4.x, int4.y, @int.x, @int.y);
			m_DepthPyramidTextureSize.x = Math.Max(m_DepthPyramidTextureSize.x, int4.x + @int.x);
			m_DepthPyramidTextureSize.y = Math.Max(m_DepthPyramidTextureSize.y, int4.y + @int.y);
		}
		while (@int.x > 1 || @int.y > 1);
		m_DepthPyramidLodCount = num + 1;
		m_DepthPyramidSamplingRatio.x = (float)viewportSize.x / (float)m_DepthPyramidTextureSize.x;
		m_DepthPyramidSamplingRatio.y = (float)viewportSize.y / (float)m_DepthPyramidTextureSize.y;
	}

	internal void Release(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(m_CameraDepthRt.Id);
		cmd.ReleaseTemporaryRT(m_CameraColorRt.Id);
		cmd.ReleaseTemporaryRT(m_CameraAlbedoRt.Id);
		cmd.ReleaseTemporaryRT(m_CameraSpecularRt.Id);
		cmd.ReleaseTemporaryRT(m_CameraNormalsRt.Id);
		cmd.ReleaseTemporaryRT(m_CameraColorPyramidRt.Id);
		cmd.ReleaseTemporaryRT(m_CameraDepthCopyRt.Id);
		cmd.ReleaseTemporaryRT(m_CameraBakedGIRt.Id);
		cmd.ReleaseTemporaryRT(m_CameraShadowmaskRT.Id);
		cmd.ReleaseTemporaryRT(m_CameraTranslucencyRT.Id);
		cmd.ReleaseTemporaryRT(m_CameraDeferredReflectionsRT.Id);
	}
}
