using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CopyDepthPass : ScriptableRenderPass<CopyDepthPassData>
{
	public enum CopyDepthMode
	{
		Final,
		Intermediate
	}

	public enum PassCullingCriteria
	{
		None,
		Opaque,
		OpaqueDistortion
	}

	private Material m_Material;

	private CopyDepthMode m_Mode;

	private Vector4 m_DepthPyramidSamplingRatio;

	private PassCullingCriteria m_CullingCriteria;

	public override string Name => "CopyDepthPass";

	public CopyDepthPass(RenderPassEvent evt, Material material, CopyDepthMode copyDepthMode, PassCullingCriteria cullingCriteria)
		: base(evt)
	{
		m_Material = material;
		m_Mode = copyDepthMode;
		m_DepthPyramidSamplingRatio = new Vector4(1f, 1f, 0f, 0f);
		m_CullingCriteria = cullingCriteria;
	}

	protected override void Setup(RenderGraphBuilder builder, CopyDepthPassData data, ref RenderingData renderingData)
	{
		switch (m_Mode)
		{
		case CopyDepthMode.Final:
			data.Input = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
			data.Output = builder.WriteTexture(in data.Resources.FinalTargetDepth);
			data.ShaderPass = (data.Resources.IsFinalTargetDepthHasDepthBits ? 1 : 0);
			data.SetGlobalTexture = false;
			break;
		case CopyDepthMode.Intermediate:
			data.Input = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
			data.Output = builder.WriteTexture(in data.Resources.CameraDepthCopyRT);
			data.ShaderPass = 0;
			data.SetGlobalTexture = true;
			break;
		}
		data.DepthPyramidSamplingRatio = m_DepthPyramidSamplingRatio;
		data.Material = m_Material;
		data.CullingCriteria = m_CullingCriteria;
	}

	protected override void Render(CopyDepthPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, data.Input);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthTexture, data.Input);
		context.cmd.SetRenderTarget(data.Output);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.ShaderPass, MeshTopology.Triangles, 3);
		if (data.SetGlobalTexture)
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, data.Output);
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthTexture, data.Output);
			context.cmd.SetGlobalVector(ShaderPropertyId._DepthPyramidSamplingRatio, data.DepthPyramidSamplingRatio);
		}
	}
}
