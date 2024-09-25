using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

internal sealed class InvokeOnRenderObjectCallbackPass : ScriptableRenderPass<PassDataBase>
{
	public override string Name => "InvokeOnRenderObjectCallbackPass";

	public InvokeOnRenderObjectCallbackPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, PassDataBase data, ref RenderingData renderingData)
	{
	}

	protected override void Render(PassDataBase data, RenderGraphContext context)
	{
		context.renderContext.InvokeOnRenderObjectCallback();
	}
}
