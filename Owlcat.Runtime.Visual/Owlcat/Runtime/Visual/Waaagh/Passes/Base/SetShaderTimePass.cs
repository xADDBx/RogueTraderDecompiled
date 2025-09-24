using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class SetShaderTimePass : ScriptableRenderPass<SetShaderTimePassData>
{
	private string m_Name;

	public override string Name => m_Name;

	public SetShaderTimePass(RenderPassEvent evt)
		: base(evt)
	{
		m_Name = string.Format("{0}.{1}", "SetShaderTimePass", evt);
	}

	protected override void Setup(RenderGraphBuilder builder, SetShaderTimePassData data, ref RenderingData renderingData)
	{
		float time = renderingData.TimeData.Time;
		float deltaTime = renderingData.TimeData.DeltaTime;
		float smoothDeltaTime = renderingData.TimeData.SmoothDeltaTime;
		float unscaledTime = renderingData.TimeData.UnscaledTime;
		float f = time / 8f;
		float f2 = time / 4f;
		float f3 = time / 2f;
		Vector4 time2 = time * new Vector4(0.05f, 1f, 2f, 3f);
		Vector4 sinTime = new Vector4(Mathf.Sin(f), Mathf.Sin(f2), Mathf.Sin(f3), Mathf.Sin(time));
		Vector4 cosTime = new Vector4(Mathf.Cos(f), Mathf.Cos(f2), Mathf.Cos(f3), Mathf.Cos(time));
		Vector4 deltaTime2 = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
		Vector4 timeParameters = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0f);
		Vector4 unscaledTime2 = unscaledTime * new Vector4(0.05f, 1f, 2f, 3f);
		Vector4 unscaledTimeParameters = new Vector4(unscaledTime, Mathf.Sin(unscaledTime), Mathf.Cos(unscaledTime), 0f);
		data.Time = time2;
		data.SinTime = sinTime;
		data.CosTime = cosTime;
		data.DeltaTime = deltaTime2;
		data.TimeParameters = timeParameters;
		data.UnscaledTime = unscaledTime2;
		data.UnscaledTimeParameters = unscaledTimeParameters;
	}

	protected override void Render(SetShaderTimePassData data, RenderGraphContext context)
	{
		CommandBuffer cmd = context.cmd;
		cmd.SetGlobalVector(ShaderPropertyId._Time, data.Time);
		cmd.SetGlobalVector(ShaderPropertyId._SinTime, data.SinTime);
		cmd.SetGlobalVector(ShaderPropertyId._CosTime, data.CosTime);
		cmd.SetGlobalVector(ShaderPropertyId.unity_DeltaTime, data.DeltaTime);
		cmd.SetGlobalVector(ShaderPropertyId._TimeParameters, data.TimeParameters);
		cmd.SetGlobalVector(ShaderPropertyId._UnscaledTime, data.UnscaledTime);
		cmd.SetGlobalVector(ShaderPropertyId._UnscaledTimeParameters, data.UnscaledTimeParameters);
	}
}
