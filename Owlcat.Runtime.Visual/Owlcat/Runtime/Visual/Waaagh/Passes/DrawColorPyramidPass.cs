using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawColorPyramidPass : ScriptableRenderPass<DrawColorPyramidPassData>
{
	private string m_Name;

	private Material m_ColorPyramidMaterial;

	private Material m_BlitMaterial;

	private ColorPyramidType m_Type;

	public override string Name => m_Name;

	public DrawColorPyramidPass(RenderPassEvent evt, ColorPyramidType type, Material colorPyramidMaterial, Material blitMaterial)
		: base(evt)
	{
		m_ColorPyramidMaterial = colorPyramidMaterial;
		m_BlitMaterial = blitMaterial;
		m_Type = type;
		m_Name = string.Format("{0}.{1}", "DrawColorPyramidPass", type);
	}

	protected override void Setup(RenderGraphBuilder builder, DrawColorPyramidPassData data, ref RenderingData renderingData)
	{
		data.Input = builder.ReadWriteTexture(in data.Resources.CameraColorBuffer);
		data.Output = builder.ReadWriteTexture(in data.Resources.CameraColorPyramidRT);
		data.TextureSize = new int2(renderingData.CameraData.CameraTargetDescriptor.width, renderingData.CameraData.CameraTargetDescriptor.height);
		data.BlitMaterial = m_BlitMaterial;
		data.ColorPyramidMaterial = m_ColorPyramidMaterial;
		switch (m_Type)
		{
		case ColorPyramidType.OpaqueDistortion:
			builder.DependsOn(in data.Resources.RendererLists.OpaqueDistortionGBuffer.List);
			break;
		case ColorPyramidType.TransparentDistortion:
			builder.DependsOn(in data.Resources.RendererLists.DistortionVectors.List);
			break;
		}
		builder.AllowRendererListCulling(value: true);
	}

	protected override void Render(DrawColorPyramidPassData data, RenderGraphContext context)
	{
		DoFast(data, context);
	}

	private static void DoHighQuality(DrawColorPyramidPassData data, RenderGraphContext context)
	{
		int num = 0;
		int num2 = data.TextureSize.x;
		int num3 = data.TextureSize.y;
		Vector4 value = new Vector4(1f, 1f, 0f, 0f);
		context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.Input);
		context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
		context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
		context.cmd.SetRenderTarget(data.Output, 0);
		context.cmd.SetViewport(new Rect(0f, 0f, num2, num3));
		context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
		int num4 = num2;
		int num5 = num3;
		while (num2 >= 8 || num3 >= 8)
		{
			int num6 = math.max(1, num2 >> 1);
			int num7 = math.max(1, num3 >> 1);
			float num8 = (float)num2 / (float)num4;
			float num9 = (float)num3 / (float)num5;
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.Output);
			context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
			context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, num);
			context.cmd.SetRenderTarget(data.TempDownsampleRT, 0);
			context.cmd.SetViewport(new Rect(0f, 0f, num6, num7));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 1, MeshTopology.Triangles, 3, 1);
			float num10 = data.TextureSize.x;
			float num11 = data.TextureSize.y;
			num8 = (float)num6 / num10;
			num9 = (float)num7 / num11;
			context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.TempDownsampleRT);
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(num8, num9, 0f, 0f));
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(((float)num6 - 0.5f) / num10, ((float)num7 - 0.5f) / num11, 1f / num10, 0f));
			context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, 0f);
			context.cmd.SetRenderTarget(data.TempColorRT, 0);
			context.cmd.SetViewport(new Rect(0f, 0f, num6, num7));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ColorPyramidMaterial, 0, MeshTopology.Triangles, 3, 1);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.TempColorRT);
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(num8, num9, 0f, 0f));
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(((float)num6 - 0.5f) / num10, ((float)num7 - 0.5f) / num11, 0f, 1f / num11));
			context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, 0f);
			context.cmd.SetRenderTarget(data.Output, num + 1);
			context.cmd.SetViewport(new Rect(0f, 0f, num6, num7));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ColorPyramidMaterial, 0, MeshTopology.Triangles, 3, 1);
			num++;
			num2 >>= 1;
			num3 >>= 1;
			num4 >>= 1;
			num5 >>= 1;
		}
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraColorPyramidRT, data.Output);
		context.cmd.SetGlobalFloat(ShaderPropertyId._ColorPyramidLodCount, num);
	}

	private void DoFast(DrawColorPyramidPassData data, RenderGraphContext context)
	{
		int num = 0;
		int num2 = data.TextureSize.x;
		int num3 = data.TextureSize.y;
		Vector4 value = new Vector4(1f, 1f, 0f, 0f);
		context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.Input);
		context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
		context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
		context.cmd.SetRenderTarget(data.Output, 0);
		context.cmd.SetViewport(new Rect(0f, 0f, num2, num3));
		context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
		while (num2 >= 8 || num3 >= 8)
		{
			int num4 = math.max(1, num2 >> 1);
			int num5 = math.max(1, num3 >> 1);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.Output);
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(1f, 1f, 0f, 0f));
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(1f, 1f, 2f / (float)num2, 0f));
			context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, num);
			context.cmd.SetRenderTarget(data.Input, 0);
			context.cmd.SetViewport(new Rect(0f, 0f, num4, num5));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ColorPyramidMaterial, 0, MeshTopology.Triangles, 3, 1);
			float x = (float)num4 / (float)data.TextureSize.x;
			float y = (float)num5 / (float)data.TextureSize.y;
			context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.Input);
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(x, y, 0f, 0f));
			context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(((float)num4 - 0.5f) / (float)data.TextureSize.x, ((float)num5 - 0.5f) / (float)data.TextureSize.y, 0f, 1f / (float)data.TextureSize.y));
			context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, 0f);
			context.cmd.SetRenderTarget(data.Output, num + 1);
			context.cmd.SetViewport(new Rect(0f, 0f, num4, num5));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ColorPyramidMaterial, 0, MeshTopology.Triangles, 3, 1);
			num++;
			num2 >>= 1;
			num3 >>= 1;
		}
		context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.Output);
		context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
		context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
		context.cmd.SetRenderTarget(data.Input, 0);
		context.cmd.SetViewport(new Rect(0f, 0f, data.TextureSize.x, data.TextureSize.y));
		context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraColorPyramidRT, data.Output);
		context.cmd.SetGlobalFloat(ShaderPropertyId._ColorPyramidLodCount, num);
	}
}
