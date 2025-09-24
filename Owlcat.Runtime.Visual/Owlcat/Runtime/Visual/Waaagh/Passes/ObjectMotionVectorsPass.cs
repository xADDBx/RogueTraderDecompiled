using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ObjectMotionVectorsPass : ScriptableRenderPass<ObjectMotionVectorsPassData>
{
	private ShaderTagId[] m_ShaderTags = new ShaderTagId[1]
	{
		new ShaderTagId("MotionVectors")
	};

	private Material m_Material;

	public override string Name => "ObjectMotionVectorsPass";

	public ObjectMotionVectorsPass(RenderPassEvent evt, Material motionVectorsMaterial)
		: base(evt)
	{
		m_Material = motionVectorsMaterial;
	}

	public DrawingSettings CreateDrawingSettings(ref RenderingData renderingData)
	{
		Camera camera = renderingData.CameraData.Camera;
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = SortingCriteria.CommonOpaque;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(m_ShaderTags[0], sortingSettings2);
		drawingSettings.perObjectData = PerObjectData.MotionVectors;
		drawingSettings.enableInstancing = true;
		drawingSettings.enableDynamicBatching = renderingData.SupportsDynamicBatching;
		DrawingSettings result = drawingSettings;
		for (int i = 1; i < m_ShaderTags.Length; i++)
		{
			result.SetShaderPassName(i, m_ShaderTags[i]);
		}
		result.fallbackMaterial = m_Material;
		return result;
	}

	protected override void Setup(RenderGraphBuilder builder, ObjectMotionVectorsPassData data, ref RenderingData renderingData)
	{
		data.MotionVectorsRT = builder.UseColorBuffer(in data.Resources.CameraMotionVectorsRT, 0);
		data.CameraDepthRT = builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.Read);
		data.DrawingSettings = CreateDrawingSettings(ref renderingData);
		data.CullingResults = renderingData.CullingResults;
		data.FilteringSettings = new FilteringSettings(WaaaghRenderQueue.Opaque);
		renderingData.CameraData.Camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
	}

	protected override void Render(ObjectMotionVectorsPassData data, RenderGraphContext context)
	{
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
		context.renderContext.DrawRenderers(data.CullingResults, ref data.DrawingSettings, ref data.FilteringSettings);
	}
}
