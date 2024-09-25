using System.Collections.Generic;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.ObjectSaturationAura.Passes;

public class ObjectSaturationAuraPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Object Saturation Aura Pass";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Object Saturation Aura Pass");

	private Material m_Material;

	private RenderTargetHandle m_SaturationAuraRT;

	private Matrix4x4[] m_Matrices;

	private float[] m_Strengths;

	private float[] m_Powers;

	public ObjectSaturationAuraPass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
		m_SaturationAuraRT.Init("_SaturationAuraRT");
		m_Matrices = new Matrix4x4[6];
	}

	public void Setup(ObjectSaturationAuraFeature feature)
	{
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		List<SaturationAuraCarrier> all = SaturationAuraCarrier.All;
		if (m_Matrices == null || m_Matrices.Length != all.Count)
		{
			m_Matrices = new Matrix4x4[all.Count];
		}
		if (m_Strengths == null || m_Strengths.Length != all.Count)
		{
			m_Strengths = new float[all.Count];
		}
		if (m_Powers == null || m_Powers.Length != all.Count)
		{
			m_Powers = new float[all.Count];
		}
		Vector3 position = renderingData.CameraData.Camera.transform.position;
		for (int i = 0; i < all.Count; i++)
		{
			SaturationAuraCarrier saturationAuraCarrier = all[i];
			Vector3 vector = position - saturationAuraCarrier.transform.position;
			vector.Normalize();
			Quaternion q = Quaternion.FromToRotation(Vector3.forward, vector);
			m_Matrices[i] = Matrix4x4.TRS(saturationAuraCarrier.transform.position + vector * saturationAuraCarrier.OffsetToCamera, q, saturationAuraCarrier.Radius * Vector3.one);
			m_Strengths[i] = (saturationAuraCarrier.Strength + 1f) * 0.5f;
			m_Powers[i] = saturationAuraCarrier.Power;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.CameraData.CameraTargetDescriptor;
			cameraTargetDescriptor.width /= 4;
			cameraTargetDescriptor.height /= 4;
			cameraTargetDescriptor.colorFormat = RenderTextureFormat.R8;
			cameraTargetDescriptor.depthBufferBits = 0;
			commandBuffer.GetTemporaryRT(m_SaturationAuraRT.Id, cameraTargetDescriptor);
			commandBuffer.SetRenderTarget(m_SaturationAuraRT.Identifier());
			commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0.5f, 0.5f, 0.5f));
			if (all.Count > 0)
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				materialPropertyBlock.SetFloatArray("_SaturationStrength", m_Strengths);
				materialPropertyBlock.SetFloatArray("_SaturationPower", m_Powers);
				commandBuffer.DrawMeshInstanced(RenderingUtils.QuadFlippedMesh, 0, m_Material, 0, m_Matrices, all.Count, materialPropertyBlock);
			}
			commandBuffer.SetGlobalTexture(m_SaturationAuraRT.Id, m_SaturationAuraRT.Identifier());
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(m_SaturationAuraRT.Id);
	}
}
