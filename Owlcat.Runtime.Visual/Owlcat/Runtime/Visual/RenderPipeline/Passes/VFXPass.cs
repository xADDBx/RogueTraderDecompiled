using System.Collections.Generic;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class VFXPass : ScriptableRenderPass
{
	private class HistoryBuffer
	{
		public int LastFrame;

		public RenderTexture Depth;
	}

	private const string kProfilerTag = "VFXPass";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("VFXPass");

	private static Dictionary<Camera, HistoryBuffer> s_HistoryBuffers = new Dictionary<Camera, HistoryBuffer>();

	private static List<Camera> s_Garbage = new List<Camera>();

	private Material m_Material;

	private RenderTargetHandle m_DepthAttachment;

	public VFXPass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
	}

	public void Setup(RenderTargetHandle depthAttachment)
	{
		m_DepthAttachment = depthAttachment;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Camera camera = renderingData.CameraData.Camera;
		if ((VFXManager.IsCameraBufferNeeded(camera) & VFXCameraBufferTypes.Depth) != 0)
		{
			HistoryBuffer buffer = GetBuffer(in renderingData.CameraData);
			buffer.LastFrame = FrameId.FrameCount;
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				commandBuffer.SetGlobalTexture(m_DepthAttachment.Id, m_DepthAttachment.Identifier());
				commandBuffer.SetRenderTarget(buffer.Depth, 0, CubemapFace.Unknown, 0);
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_Material, 0, MeshTopology.Triangles, 3);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
			VFXManager.SetCameraBuffer(camera, VFXCameraBufferTypes.Depth, buffer.Depth, 0, 0, buffer.Depth.width, buffer.Depth.height);
		}
		s_Garbage.Clear();
		foreach (KeyValuePair<Camera, HistoryBuffer> s_HistoryBuffer in s_HistoryBuffers)
		{
			if (FrameId.FrameCount - s_HistoryBuffer.Value.LastFrame > 4)
			{
				RenderTexture.ReleaseTemporary(s_HistoryBuffer.Value.Depth);
				s_Garbage.Add(s_HistoryBuffer.Key);
			}
		}
		foreach (Camera item in s_Garbage)
		{
			s_HistoryBuffers.Remove(item);
		}
	}

	private HistoryBuffer GetBuffer(in CameraData cameraData)
	{
		if (!s_HistoryBuffers.TryGetValue(cameraData.Camera, out var value))
		{
			value = new HistoryBuffer();
			s_HistoryBuffers[cameraData.Camera] = value;
		}
		if (value.Depth == null || value.Depth.width != cameraData.CameraTargetDescriptor.width || value.Depth.height != cameraData.CameraTargetDescriptor.height)
		{
			if (value.Depth != null)
			{
				RenderTexture.ReleaseTemporary(value.Depth);
			}
			RenderTextureDescriptor cameraTargetDescriptor = cameraData.CameraTargetDescriptor;
			cameraTargetDescriptor.depthBufferBits = 0;
			cameraTargetDescriptor.colorFormat = RenderTextureFormat.RFloat;
			cameraTargetDescriptor.dimension = TextureDimension.Tex2DArray;
			cameraTargetDescriptor.volumeDepth = 1;
			value.Depth = RenderTexture.GetTemporary(cameraTargetDescriptor);
			value.Depth.name = $"{cameraData.Camera}_HistoryDepthBuffer";
		}
		return value;
	}

	public static void ClearBuffers()
	{
		foreach (KeyValuePair<Camera, HistoryBuffer> s_HistoryBuffer in s_HistoryBuffers)
		{
			RenderTexture.ReleaseTemporary(s_HistoryBuffer.Value.Depth);
		}
		s_HistoryBuffers.Clear();
	}
}
