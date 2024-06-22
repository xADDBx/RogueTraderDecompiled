using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class SetupDebugBuffersPass : ScriptableRenderPass<SetupDebugBuffersPass.PassData>
{
	public sealed class PassData : PassDataBase
	{
		public ComputeBufferHandle FullScreenDebugBuffer;

		public int2 FullScreenDebugBufferDimensions;

		public ComputeShader ClearDebugBuffersShader;

		public int ClearDebugBuffersShaderMainKernel;

		public int ClearDebugBuffersThreadGroupsX;
	}

	private readonly RenderGraphDebugResources m_Resources;

	private readonly WaaaghDebugData m_DebugData;

	private readonly ComputeShader m_ClearDebugBuffersShader;

	private readonly int m_ClearDebugBuffersShaderMainKernel;

	private readonly uint m_ClearDebugBuffersShaderMainKernelThreadGroupSizeX;

	public override string Name => "SetupDebugBuffersPass";

	internal SetupDebugBuffersPass(RenderGraphDebugResources resources, WaaaghDebugData debugData)
		: base(RenderPassEvent.BeforeRendering)
	{
		m_Resources = resources;
		m_DebugData = debugData;
		m_ClearDebugBuffersShader = debugData.Shaders.ClearDebugBuffersCS;
		m_ClearDebugBuffersShaderMainKernel = m_ClearDebugBuffersShader.FindKernel("main");
		m_ClearDebugBuffersShader.GetKernelThreadGroupSizes(m_ClearDebugBuffersShaderMainKernel, out m_ClearDebugBuffersShaderMainKernelThreadGroupSizeX, out var _, out var _);
	}

	protected override void Setup(RenderGraphBuilder builder, PassData data, ref RenderingData renderingData)
	{
		int2 fullScreenDebugBufferDimensions = new int2(renderingData.CameraData.ScaledCameraTargetViewportSize.x, renderingData.CameraData.ScaledCameraTargetViewportSize.y);
		int num = fullScreenDebugBufferDimensions.x * fullScreenDebugBufferDimensions.y;
		ComputeBufferDesc computeBufferDesc = default(ComputeBufferDesc);
		computeBufferDesc.count = num;
		computeBufferDesc.stride = 4;
		computeBufferDesc.type = ComputeBufferType.Structured;
		computeBufferDesc.name = "FullScreenDebug";
		ComputeBufferDesc desc = computeBufferDesc;
		m_Resources.FullScreenDebugBuffer = renderingData.RenderGraph.CreateComputeBuffer(in desc);
		m_Resources.FullScreenDebugBufferDimensions = fullScreenDebugBufferDimensions;
		data.FullScreenDebugBuffer = m_Resources.FullScreenDebugBuffer;
		data.FullScreenDebugBufferDimensions = m_Resources.FullScreenDebugBufferDimensions;
		data.ClearDebugBuffersShader = m_ClearDebugBuffersShader;
		data.ClearDebugBuffersShaderMainKernel = m_ClearDebugBuffersShaderMainKernel;
		data.ClearDebugBuffersThreadGroupsX = Mathf.CeilToInt((float)num / (float)m_ClearDebugBuffersShaderMainKernelThreadGroupSizeX);
		builder.AllowPassCulling(value: false);
		builder.WriteComputeBuffer(in m_Resources.FullScreenDebugBuffer);
	}

	protected override void Render(PassData data, RenderGraphContext context)
	{
		context.cmd.SetRandomWriteTarget(1, data.FullScreenDebugBuffer);
		context.cmd.SetComputeBufferParam(data.ClearDebugBuffersShader, data.ClearDebugBuffersShaderMainKernel, "_FullScreenDebugBuffer", data.FullScreenDebugBuffer);
		context.cmd.SetGlobalVector("_FullScreenDebugBufferParams", new Vector4(data.FullScreenDebugBufferDimensions.x, data.FullScreenDebugBufferDimensions.y, data.FullScreenDebugBufferDimensions.x * data.FullScreenDebugBufferDimensions.y, 0f));
		context.cmd.SetGlobalFloat("_QuadOverdrawMaxQuadCost", m_DebugData.RenderingDebug.QuadOverdrawSettings.MaxQuadCost);
		context.cmd.DispatchCompute(data.ClearDebugBuffersShader, data.ClearDebugBuffersShaderMainKernel, data.ClearDebugBuffersThreadGroupsX, 1, 1);
	}
}
