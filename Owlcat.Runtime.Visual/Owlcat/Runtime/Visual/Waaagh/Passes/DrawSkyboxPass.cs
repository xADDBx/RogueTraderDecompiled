using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawSkyboxPass : ScriptableRenderPass<DrawSkyboxPassData>
{
	public override string Name => "DrawSkyboxPass";

	public DrawSkyboxPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, DrawSkyboxPassData data, ref RenderingData renderingData)
	{
		data.ColorOutput = builder.WriteTexture(in data.Resources.CameraColorBuffer);
		data.DepthOutput = builder.WriteTexture(in data.Resources.CameraDepthBuffer);
		data.Camera = renderingData.CameraData.Camera;
	}

	protected override void Render(DrawSkyboxPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.ColorOutput, data.DepthOutput);
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
		context.renderContext.DrawSkybox(data.Camera);
	}
}
