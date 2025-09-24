using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CapturePass : ScriptableRenderPass<CapturePassData>
{
	public override string Name => "CapturePass";

	public CapturePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, CapturePassData data, ref RenderingData renderingData)
	{
		data.CaptureActions = renderingData.CaptureActions;
		data.CameraColorBuffer = builder.ReadWriteTexture(in data.Resources.CameraColorBuffer);
	}

	protected override void Render(CapturePassData data, RenderGraphContext context)
	{
		data.CaptureActions.Reset();
		while (data.CaptureActions.MoveNext())
		{
			data.CaptureActions.Current(data.CameraColorBuffer, context.cmd);
		}
	}
}
