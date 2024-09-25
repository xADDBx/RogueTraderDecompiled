using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class VFXPreparePass : ScriptableRenderPass<VFXPreparePassData>
{
	public override string Name => "VFXPreparePass";

	public VFXPreparePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, VFXPreparePassData data, ref RenderingData renderingData)
	{
		data.Camera = renderingData.CameraData.Camera;
		data.CullingResults = renderingData.CullingResults;
	}

	protected override void Render(VFXPreparePassData data, RenderGraphContext context)
	{
		VFXManager.ProcessCameraCommand(data.Camera, context.cmd, default(VFXCameraXRSettings), data.CullingResults);
	}
}
