using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class ScreenSpaceCloudShadowsPass : ScriptableRenderPass
{
	public static class ConstantBuffer
	{
		public static int _Texture0 = Shader.PropertyToID("_Texture0");

		public static int _Texture0ScaleBias = Shader.PropertyToID("_Texture0ScaleBias");

		public static int _Texture1 = Shader.PropertyToID("_Texture1");

		public static int _Texture1ScaleBias = Shader.PropertyToID("_Texture1ScaleBias");

		public static int _Texture0Color = Shader.PropertyToID("_Texture0Color");

		public static int _Texture1Color = Shader.PropertyToID("_Texture1Color");

		public static int _Intensity = Shader.PropertyToID("_Intensity");
	}

	private const string kProfilerTag = "Draw Screen Space Cloud Shadows";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw Screen Space Cloud Shadows");

	private Material m_Material;

	private RenderTargetHandle m_ColorAttachment;

	private ScreenSpaceCloudShadows m_Settings;

	private Vector2 m_Scroll0;

	private Vector2 m_Scroll1;

	public ScreenSpaceCloudShadowsPass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
	}

	internal void Setup(RenderTargetHandle colorAttachment)
	{
		m_ColorAttachment = colorAttachment;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_Settings = stack.GetComponent<ScreenSpaceCloudShadows>();
		if (m_Settings.IsActive())
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				commandBuffer.SetGlobalTexture(ConstantBuffer._Texture0, m_Settings.Texture0.value);
				commandBuffer.SetGlobalTexture(ConstantBuffer._Texture1, m_Settings.Texture1.value);
				Vector4 value = default(Vector4);
				Vector2 value2 = m_Settings.Texture0Tiling.value;
				m_Scroll0 += m_Settings.Texture0ScrollSpeed.value * Time.deltaTime;
				value.x = value2.x;
				value.y = value2.y;
				value.z = m_Scroll0.x;
				value.w = m_Scroll0.y;
				commandBuffer.SetGlobalVector(ConstantBuffer._Texture0ScaleBias, value);
				commandBuffer.SetGlobalVector(ConstantBuffer._Texture0Color, m_Settings.Texture0Color.value);
				Vector4 value3 = default(Vector4);
				value2 = m_Settings.Texture1Tiling.value;
				m_Scroll1 += m_Settings.Texture1ScrollSpeed.value * Time.deltaTime;
				value3.x = value2.x;
				value3.y = value2.y;
				value3.z = m_Scroll1.x;
				value3.w = m_Scroll1.y;
				commandBuffer.SetGlobalVector(ConstantBuffer._Texture1Color, m_Settings.Texture1Color.value);
				commandBuffer.SetGlobalVector(ConstantBuffer._Texture1ScaleBias, value3);
				commandBuffer.SetGlobalFloat(ConstantBuffer._Intensity, m_Settings.Intensity.value);
				commandBuffer.SetRenderTarget(m_ColorAttachment.Identifier());
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_Material, 0, MeshTopology.Triangles, 3);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}
}
