using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class DrawGizmosPass : ScriptableRenderPass<DrawGizmosPassData>
{
	private GizmoSubset m_GizmoSubset;

	private string m_Name;

	public override string Name => m_Name;

	public DrawGizmosPass(GizmoSubset gizmoSubset)
		: base((gizmoSubset == GizmoSubset.PreImageEffects) ? RenderPassEvent.AfterRenderingTransparents : ((RenderPassEvent)1001))
	{
		m_GizmoSubset = gizmoSubset;
		m_Name = string.Format("{0}.{1}", "DrawGizmosPass", m_GizmoSubset);
	}

	protected override void Setup(RenderGraphBuilder builder, DrawGizmosPassData data, ref RenderingData renderingData)
	{
		data.Camera = renderingData.CameraData.Camera;
		data.GizmoSubset = m_GizmoSubset;
	}

	protected override void Render(DrawGizmosPassData data, RenderGraphContext context)
	{
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
		context.renderContext.DrawGizmos(data.Camera, data.GizmoSubset);
	}
}
