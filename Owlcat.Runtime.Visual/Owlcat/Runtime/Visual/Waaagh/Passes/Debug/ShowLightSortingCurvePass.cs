using Owlcat.Runtime.Visual.Waaagh.Debugging;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class ShowLightSortingCurvePass : ScriptableRenderPass<ShowLightSortingCurvePassData>
{
	private static readonly int _ColorStart = Shader.PropertyToID("_ColorStart");

	private static readonly int _ColorEnd = Shader.PropertyToID("_ColorEnd");

	private WaaaghRenderer m_Renderer;

	private WaaaghDebugData m_DebugData;

	private Material m_Material;

	public override string Name => "ShowLightSortingCurvePass";

	public ShowLightSortingCurvePass(RenderPassEvent evt, WaaaghRenderer renderer, WaaaghDebugData debugData, Material material)
		: base(evt)
	{
		m_Renderer = renderer;
		m_Material = material;
		m_DebugData = debugData;
	}

	protected override void Setup(RenderGraphBuilder builder, ShowLightSortingCurvePassData data, ref RenderingData renderingData)
	{
		data.Material = m_Material;
		data.TotalLightsCount = (int)m_Renderer.WaaaghLights.LightDataParams.z;
		data.ColorStart = m_DebugData.LightingDebug.LightSortingCurveColorStart;
		data.ColorEnd = m_DebugData.LightingDebug.LightSortingCurveColorEnd;
	}

	protected override void Render(ShowLightSortingCurvePassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalColor(_ColorStart, data.ColorStart);
		context.cmd.SetGlobalColor(_ColorEnd, data.ColorEnd);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Lines, 2, data.TotalLightsCount - 1);
	}
}
