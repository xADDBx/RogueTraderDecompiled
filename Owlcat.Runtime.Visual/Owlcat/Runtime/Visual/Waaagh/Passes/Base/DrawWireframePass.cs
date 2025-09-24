using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class DrawWireframePass : ScriptableRenderPass<DrawWireframePassData>
{
	public override string Name => "DrawWireframePass";

	public DrawWireframePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, DrawWireframePassData data, ref RenderingData renderingData)
	{
		data.Camera = renderingData.CameraData.Camera;
	}

	protected override void Render(DrawWireframePassData data, RenderGraphContext context)
	{
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
		context.renderContext.DrawWireOverlay(data.Camera);
	}
}
