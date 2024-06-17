using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class DrawColorPyramidPass : ScriptableRenderPass
{
	private const string kColorPyramidTag = "Render Color Pyramid";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Render Color Pyramid");

	private RenderTextureDescriptor m_ColorPyramidDesc;

	private RenderTargetHandle m_Source;

	private Material m_ColorPyramidMat;

	private Material m_BlitMat;

	private RenderTargetHandle m_Dest;

	private RenderTargetHandle m_TempDownsampleRt;

	private RenderTargetHandle m_TempColorRt;

	public DrawColorPyramidPass(RenderPassEvent evt, Material colorPyramidMaterial, Material blitMaterial)
	{
		base.RenderPassEvent = evt;
		m_ColorPyramidMat = colorPyramidMaterial;
		m_BlitMat = blitMaterial;
		m_TempDownsampleRt.Init("_TempDownsampleRt");
		m_TempColorRt.Init("_TempColorRt");
	}

	public void Setup(RenderTextureDescriptor sourceDescriptor, RenderTextureFormat sourceColorFormat, RenderTargetHandle source, RenderTargetHandle destination)
	{
		m_ColorPyramidDesc = sourceDescriptor;
		m_ColorPyramidDesc.useMipMap = true;
		m_ColorPyramidDesc.autoGenerateMips = false;
		m_ColorPyramidDesc.depthBufferBits = 0;
		m_ColorPyramidDesc.colorFormat = sourceColorFormat;
		m_Source = source;
		m_Dest = destination;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			int num = 0;
			int num2 = m_ColorPyramidDesc.width;
			int num3 = m_ColorPyramidDesc.height;
			Vector4 value = new Vector4(1f, 1f, 0f, 0f);
			commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, m_Source.Identifier());
			commandBuffer.SetGlobalVector(BlitBuffer._BlitScaleBias, value);
			commandBuffer.SetGlobalFloat(BlitBuffer._BlitMipLevel, 0f);
			commandBuffer.SetRenderTarget(m_Dest.Identifier(), 0);
			commandBuffer.SetViewport(new Rect(0f, 0f, num2, num3));
			commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMat, 0, MeshTopology.Triangles, 3, 1);
			int num4 = m_ColorPyramidDesc.width;
			int num5 = m_ColorPyramidDesc.height;
			commandBuffer.GetTemporaryRT(m_TempDownsampleRt.Id, m_ColorPyramidDesc);
			commandBuffer.GetTemporaryRT(m_TempColorRt.Id, m_ColorPyramidDesc);
			while (num2 >= 8 || num3 >= 8)
			{
				int num6 = Mathf.Max(1, num2 >> 1);
				int num7 = Mathf.Max(1, num3 >> 1);
				float num8 = (float)num2 / (float)num4;
				float num9 = (float)num3 / (float)num5;
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, m_Dest.Identifier());
				commandBuffer.SetGlobalVector(BlitBuffer._BlitScaleBias, value);
				commandBuffer.SetGlobalFloat(BlitBuffer._BlitMipLevel, num);
				commandBuffer.SetRenderTarget(m_TempDownsampleRt.Identifier(), 0);
				commandBuffer.SetViewport(new Rect(0f, 0f, num6, num7));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMat, 1, MeshTopology.Triangles, 3, 1);
				float num10 = m_ColorPyramidDesc.width;
				float num11 = m_ColorPyramidDesc.height;
				num8 = (float)num6 / num10;
				num9 = (float)num7 / num11;
				commandBuffer.SetGlobalTexture(ColorPyramidBuffer._Source, m_TempDownsampleRt.Identifier());
				commandBuffer.SetGlobalVector(ColorPyramidBuffer._SrcScaleBias, new Vector4(num8, num9, 0f, 0f));
				commandBuffer.SetGlobalVector(ColorPyramidBuffer._SrcUvLimits, new Vector4(((float)num6 - 0.5f) / num10, ((float)num7 - 0.5f) / num11, 1f / num10, 0f));
				commandBuffer.SetGlobalFloat(ColorPyramidBuffer._SourceMip, 0f);
				commandBuffer.SetRenderTarget(m_TempColorRt.Identifier(), 0);
				commandBuffer.SetViewport(new Rect(0f, 0f, num6, num7));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_ColorPyramidMat, 0, MeshTopology.Triangles, 3, 1);
				commandBuffer.SetGlobalTexture(ColorPyramidBuffer._Source, m_TempColorRt.Identifier());
				commandBuffer.SetGlobalVector(ColorPyramidBuffer._SrcScaleBias, new Vector4(num8, num9, 0f, 0f));
				commandBuffer.SetGlobalVector(ColorPyramidBuffer._SrcUvLimits, new Vector4(((float)num6 - 0.5f) / num10, ((float)num7 - 0.5f) / num11, 0f, 1f / num11));
				commandBuffer.SetGlobalFloat(ColorPyramidBuffer._SourceMip, 0f);
				commandBuffer.SetRenderTarget(m_Dest.Identifier(), num + 1);
				commandBuffer.SetViewport(new Rect(0f, 0f, num6, num7));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_ColorPyramidMat, 0, MeshTopology.Triangles, 3, 1);
				num++;
				num2 >>= 1;
				num3 >>= 1;
				num4 >>= 1;
				num5 >>= 1;
			}
			commandBuffer.ReleaseTemporaryRT(m_TempDownsampleRt.Id);
			commandBuffer.ReleaseTemporaryRT(m_TempColorRt.Id);
			commandBuffer.SetGlobalTexture(m_Dest.Id, m_Dest.Identifier());
			commandBuffer.SetGlobalFloat(ColorPyramidBuffer._ColorPyramidLodCount, num);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
