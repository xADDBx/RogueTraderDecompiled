using System;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DepthPyramidPass : ScriptableRenderPass<DepthPyramidPassData>
{
	public static class ShaderConstantsId
	{
		public static readonly int _InputDepthTex = Shader.PropertyToID("_InputDepthTex");

		public static readonly int _SrcOffsetAndLimit = Shader.PropertyToID("_SrcOffsetAndLimit");

		public static readonly int _DstOffset = Shader.PropertyToID("_DstOffset");

		public static readonly int _CameraDepthUAVSize = Shader.PropertyToID("_CameraDepthUAVSize");

		public static readonly int _CameraDepthUAV = Shader.PropertyToID("_CameraDepthUAV");

		public static readonly int _DepthPyramidSamplingRatio = Shader.PropertyToID("_DepthPyramidSamplingRatio");

		public static readonly int _DepthPyramidMipRects = Shader.PropertyToID("_DepthPyramidMipRects");

		public static readonly int _DepthPyramidLodCount = Shader.PropertyToID("_DepthPyramidLodCount");
	}

	private const int kMaxDepthPyramidLevelCount = 16;

	private ComputeShader m_Shader;

	private ComputeShaderKernelDescriptor m_Kernel;

	private Material m_CopyDepthMaterial;

	private int2 m_DepthPyramidTextureSize;

	private Vector4[] m_DepthPyramidMipRects = new Vector4[16];

	private int m_DepthPyramidLodCount;

	private Vector4 m_DepthPyramidSamplingRatio;

	private int[] m_SrcOffset = new int[4];

	private int[] m_DstOffset = new int[4];

	private LocalKeyword m_READ_FROM_CAMERA_DEPTH;

	public override string Name => "DepthPyramidPass";

	public DepthPyramidPass(RenderPassEvent evt, ComputeShader shader, Material copyDepthMaterial)
		: base(evt)
	{
		m_Shader = shader;
		m_Kernel = m_Shader.GetKernelDescriptor("DepthPyramid");
		m_CopyDepthMaterial = copyDepthMaterial;
		m_READ_FROM_CAMERA_DEPTH = new LocalKeyword(m_Shader, "READ_FROM_CAMERA_DEPTH");
	}

	protected override void Setup(RenderGraphBuilder builder, DepthPyramidPassData data, ref RenderingData renderingData)
	{
		int2 viewportSize = new int2(renderingData.CameraData.ScaledCameraTargetViewportSize.x, renderingData.CameraData.ScaledCameraTargetViewportSize.y);
		ComputePackedMipChainInfo(viewportSize);
		TextureDesc desc = new TextureDesc(m_DepthPyramidTextureSize.x, m_DepthPyramidTextureSize.y);
		desc.colorFormat = GraphicsFormat.R32_SFloat;
		desc.depthBufferBits = DepthBits.None;
		desc.enableRandomWrite = true;
		data.Resources.CameraDepthPyramidRT = renderingData.RenderGraph.CreateTexture(in desc);
		data.DepthPyramidUAV = builder.ReadWriteTexture(in data.Resources.CameraDepthPyramidRT);
		data.CameraDepthBuffer = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
		data.Shader = m_Shader;
		data.Kernel = m_Kernel;
		data.CopyDepthMaterial = m_CopyDepthMaterial;
		data.READ_FROM_CAMERA_DEPTH = m_READ_FROM_CAMERA_DEPTH;
		data.DepthPyramidTextureSize = m_DepthPyramidTextureSize;
		data.DepthPyramidLodCount = m_DepthPyramidLodCount;
		data.DepthPyramidMipRects = m_DepthPyramidMipRects;
		data.DepthPyramidSamplingRatio = m_DepthPyramidSamplingRatio;
		data.SrcOffset = m_SrcOffset;
		data.DstOffset = m_DstOffset;
		data.Viewport = new Rect(0f, 0f, viewportSize.x, viewportSize.y);
	}

	protected override void Render(DepthPyramidPassData data, RenderGraphContext context)
	{
		if (FrameDebugger.enabled)
		{
			context.cmd.SetRenderTarget(data.DepthPyramidUAV);
		}
		for (int i = 1; i < data.DepthPyramidLodCount; i++)
		{
			int4 @int = (int4)data.DepthPyramidMipRects[i];
			int4 int2 = (int4)data.DepthPyramidMipRects[i - 1];
			data.SrcOffset[0] = int2.x;
			data.SrcOffset[1] = int2.y;
			data.SrcOffset[2] = int2.x + int2.z - 1;
			data.SrcOffset[3] = int2.y + int2.w - 1;
			data.DstOffset[0] = @int.x;
			data.DstOffset[1] = @int.y;
			data.DstOffset[2] = 0;
			data.DstOffset[3] = 0;
			context.cmd.SetComputeIntParams(data.Shader, ShaderConstantsId._SrcOffsetAndLimit, m_SrcOffset);
			context.cmd.SetComputeIntParams(data.Shader, ShaderConstantsId._DstOffset, m_DstOffset);
			int2 depthPyramidTextureSize = data.DepthPyramidTextureSize;
			context.cmd.SetComputeVectorParam(data.Shader, ShaderConstantsId._CameraDepthUAVSize, new Vector4(depthPyramidTextureSize.x, depthPyramidTextureSize.y));
			context.cmd.SetComputeTextureParam(data.Shader, data.Kernel.Index, ShaderConstantsId._CameraDepthUAV, data.DepthPyramidUAV);
			context.cmd.SetKeyword(data.Shader, in data.READ_FROM_CAMERA_DEPTH, i == 1);
			if (i == 1)
			{
				context.cmd.SetComputeTextureParam(data.Shader, data.Kernel.Index, ShaderPropertyId._CameraDepthRT, data.CameraDepthBuffer);
			}
			context.cmd.DispatchCompute(data.Shader, data.Kernel.Index, RenderingUtils.DivRoundUp(@int.z, (int)data.Kernel.ThreadGroupSize.x), RenderingUtils.DivRoundUp(@int.w, (int)data.Kernel.ThreadGroupSize.y), 1);
		}
		context.cmd.SetGlobalVector(ShaderConstantsId._DepthPyramidSamplingRatio, data.DepthPyramidSamplingRatio);
		context.cmd.SetGlobalVectorArray(ShaderConstantsId._DepthPyramidMipRects, data.DepthPyramidMipRects);
		context.cmd.SetGlobalInt(ShaderConstantsId._DepthPyramidLodCount, data.DepthPyramidLodCount);
	}

	public void ComputePackedMipChainInfo(int2 viewportSize)
	{
		m_DepthPyramidTextureSize = viewportSize >> 1;
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
			if (num > 1)
			{
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
}
