using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar.Passes;

public class FogOfWarCleanupPass : ScriptableRenderPass<PassDataBase>
{
	public override string Name => "FogOfWarCleanupPass";

	public FogOfWarCleanupPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, PassDataBase data, ref RenderingData renderingData)
	{
	}

	protected override void Render(PassDataBase data, RenderGraphContext context)
	{
		context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, 0f);
	}
}
