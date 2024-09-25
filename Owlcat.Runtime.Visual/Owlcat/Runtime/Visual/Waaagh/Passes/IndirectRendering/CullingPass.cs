using Owlcat.Runtime.Visual.IndirectRendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.IndirectRendering;

public class CullingPass : ScriptableRenderPass<CullingPassData>
{
	public override string Name => "CullingPass";

	public CullingPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, CullingPassData data, ref RenderingData renderingData)
	{
		data.Camera = renderingData.CameraData.Camera;
	}

	protected override void Render(CullingPassData data, RenderGraphContext context)
	{
		IndirectRenderingSystem.Instance.Cull(ref context.renderContext, data.Camera);
	}
}
