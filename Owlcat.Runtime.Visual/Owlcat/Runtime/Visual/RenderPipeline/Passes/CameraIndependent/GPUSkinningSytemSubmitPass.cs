using Owlcat.Runtime.Visual.RenderPipeline.GPUSkinning;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes.CameraIndependent;

public class GPUSkinningSytemSubmitPass : ScriptableRenderPass
{
	public GPUSkinningSytemSubmitPass(RenderPassEvent evt)
	{
		base.RenderPassEvent = evt;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		GPUSkinningSystem.Instance.Submit();
	}
}
