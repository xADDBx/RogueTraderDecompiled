using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CameraMotionVectorsPass : ScriptableRenderPass<CameraMotionVectorsPassData>
{
	private Material m_CameraMotionVectorsMaterial;

	public override string Name => "CameraMotionVectorsPass";

	public CameraMotionVectorsPass(RenderPassEvent evt, Material cameraMotionVectorsMaterial)
		: base(evt)
	{
		m_CameraMotionVectorsMaterial = cameraMotionVectorsMaterial;
	}

	protected override void Setup(RenderGraphBuilder builder, CameraMotionVectorsPassData data, ref RenderingData renderingData)
	{
		TextureDesc desc = RenderingUtils.CreateTextureDesc("CameraMotionVectorsRT", renderingData.CameraData.CameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R16G16_SFloat;
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		data.Resources.CameraMotionVectorsRT = renderingData.RenderGraph.CreateTexture(in desc);
		data.CameraMotionVectors = builder.WriteTexture(in data.Resources.CameraMotionVectorsRT);
		data.CameraDepthRT = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
		data.Material = m_CameraMotionVectorsMaterial;
		Matrix4x4 gPUProjectionMatrixNoJitter = renderingData.CameraData.GetGPUProjectionMatrixNoJitter();
		Matrix4x4 viewMatrix = renderingData.CameraData.GetViewMatrix();
		data.UnjitteredVP = CoreMatrixUtils.MultiplyProjectionMatrix(gPUProjectionMatrixNoJitter, viewMatrix, renderingData.CameraData.Camera.orthographic);
		data.PrevVP = renderingData.CameraData.CameraBuffer.PreviuosViewProjectionMatrix;
	}

	protected override void Render(CameraMotionVectorsPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalMatrix(ShaderPropertyId._NonJitteredViewProjMatrix, data.UnjitteredVP);
		context.cmd.SetGlobalMatrix(ShaderPropertyId._PrevViewProjMatrix, data.PrevVP);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, data.CameraDepthRT);
		context.cmd.SetRenderTarget(data.CameraMotionVectors);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}
}
