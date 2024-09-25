using System;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.OccludedObjectHighlighting.Passes;

public class OccludedObjectDepthClipperPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Occluded Object Depth Clip Pass";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Occluded Object Depth Clip Pass");

	private Material m_Material;

	private RenderTargetHandle m_OccludedDepthRT;

	private Matrix4x4[] m_Matrices;

	private float m_NoiseTiling;

	private float m_DepthClipTreshold;

	private float m_OccludedObjectAlphaScale;

	private float m_NearCameraClipDistance;

	public OccludedObjectDepthClipperPass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
		m_OccludedDepthRT.Init("_OccludedDepthRT");
		m_Matrices = new Matrix4x4[6];
	}

	internal void Setup(OccludedObjectHighlightingFeature feature)
	{
		m_DepthClipTreshold = feature.DepthClip.ClipTreshold;
		m_NoiseTiling = feature.DepthClip.NoiseTiling;
		m_OccludedObjectAlphaScale = feature.DepthClip.AlphaScale;
		m_NearCameraClipDistance = feature.DepthClip.NearCameraClipDistance;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_Matrices.Length < OccludedObjectDepthClipper.All.Count)
		{
			int newSize = (int)((float)OccludedObjectDepthClipper.All.Count * 1.5f);
			Array.Resize(ref m_Matrices, newSize);
		}
		Vector3 position = renderingData.CameraData.Camera.transform.position;
		int num = 0;
		foreach (OccludedObjectDepthClipper item in OccludedObjectDepthClipper.All)
		{
			Vector3 vector = position - item.transform.position;
			vector.Normalize();
			Quaternion q = Quaternion.FromToRotation(Vector3.forward, vector);
			m_Matrices[num] = Matrix4x4.TRS(item.transform.position + vector * item.OffsetToCamera, q, item.Radius * Vector3.one);
			num++;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.CameraData.CameraTargetDescriptor;
			cameraTargetDescriptor.width /= 4;
			cameraTargetDescriptor.height /= 4;
			cameraTargetDescriptor.colorFormat = RenderTextureFormat.RGHalf;
			cameraTargetDescriptor.depthBufferBits = 0;
			commandBuffer.GetTemporaryRT(m_OccludedDepthRT.Id, cameraTargetDescriptor);
			commandBuffer.SetRenderTarget(m_OccludedDepthRT.Identifier());
			commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
			if (OccludedObjectDepthClipper.All.Count > 0)
			{
				commandBuffer.DrawMeshInstanced(RenderingUtils.QuadFlippedMesh, 0, m_Material, 0, m_Matrices, OccludedObjectDepthClipper.All.Count);
			}
			commandBuffer.SetGlobalFloat(CameraBuffer._OccludedObjectAlphaScale, m_OccludedObjectAlphaScale);
			commandBuffer.SetGlobalFloat(CameraBuffer._OccludedObjectClipNoiseTiling, m_NoiseTiling);
			commandBuffer.SetGlobalFloat(CameraBuffer._OccludedObjectClipTreshold, m_DepthClipTreshold);
			commandBuffer.SetGlobalFloat(CameraBuffer._OccludedObjectClipNearCameraDistance, m_NearCameraClipDistance);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(m_OccludedDepthRT.Id);
	}
}
