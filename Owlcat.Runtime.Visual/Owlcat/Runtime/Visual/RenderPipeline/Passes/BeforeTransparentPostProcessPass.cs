using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class BeforeTransparentPostProcessPass : ScriptableRenderPass
{
	private class MaterialLibrary
	{
		public readonly Material MaskedColorTransform;

		public readonly Material SaturationOverlay;

		public MaterialLibrary(PostProcessData data)
		{
			MaskedColorTransform = Load(data.Shaders.MaskedColorTransformPS);
			SaturationOverlay = Load(data.Shaders.SaturationOverlayPS);
		}

		private Material Load(Shader shader)
		{
			if (shader == null)
			{
				Debug.LogErrorFormat("Missing shader. " + GetType().DeclaringType.Name + " render pass will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			return CoreUtils.CreateEngineMaterial(shader);
		}
	}

	private static class ShaderConstants
	{
		public static readonly int _TempTarget = Shader.PropertyToID("_TempTarget");

		public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static readonly int _MainTex = Shader.PropertyToID("_MainTex");

		public static readonly int _MaskedColorTransformParams = Shader.PropertyToID("_MaskedColorTransformParams");
	}

	private const string kProfilerTag = "Before Transparent Post Process Pass";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Before Transparent Post Process Pass");

	private RenderTextureDescriptor m_Descriptor;

	private RenderTargetHandle m_Source;

	private RenderTargetHandle m_Destination;

	private RenderTargetHandle m_Depth;

	private MaterialLibrary m_Materials;

	private Material m_BlitMaterial;

	private MaskedColorTransform m_MaskedColorTransform;

	private SaturationOverlay m_SaturationOverlay;

	public BeforeTransparentPostProcessPass(RenderPassEvent evt, PostProcessData data, Material blitMaterial)
	{
		base.RenderPassEvent = evt;
		m_Materials = new MaterialLibrary(data);
		m_BlitMaterial = blitMaterial;
	}

	public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle source, RenderTargetHandle destination, RenderTargetHandle depth)
	{
		m_Descriptor = baseDescriptor;
		m_Source = source;
		m_Destination = destination;
		m_Depth = depth;
	}

	private RenderTextureDescriptor GetCompatibleDescriptor()
	{
		return GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
	}

	private RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format)
	{
		RenderTextureDescriptor descriptor = m_Descriptor;
		descriptor.depthBufferBits = 0;
		descriptor.msaaSamples = 1;
		descriptor.width = width;
		descriptor.height = height;
		descriptor.graphicsFormat = format;
		return descriptor;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_MaskedColorTransform = stack.GetComponent<MaskedColorTransform>();
		m_SaturationOverlay = stack.GetComponent<SaturationOverlay>();
		CommandBuffer cmd = CommandBufferPool.Get();
		int source = m_Source.Id;
		int destination = -1;
		using (new ProfilingScope(cmd, m_ProfilingSampler))
		{
			if (m_MaskedColorTransform.IsActive())
			{
				DoMaskedColorTransform(cmd, GetSource(), GetDestination());
				Swap();
			}
			if (m_SaturationOverlay.IsActive())
			{
				DoSaturationOverlay(cmd, GetSource(), GetDestination());
				Swap();
			}
			if (destination != -1 && GetSource() != m_Destination.Id)
			{
				Blit(cmd, GetSource(), -1, m_Destination.Id, m_BlitMaterial, 0);
			}
			if (destination != -1)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._TempTarget);
			}
		}
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
		int GetDestination()
		{
			if (destination == -1)
			{
				cmd.GetTemporaryRT(ShaderConstants._TempTarget, GetCompatibleDescriptor(), FilterMode.Bilinear);
				destination = ShaderConstants._TempTarget;
			}
			return destination;
		}
		int GetSource()
		{
			return source;
		}
		void Swap()
		{
			CoreUtils.Swap(ref source, ref destination);
		}
	}

	private void Blit(CommandBuffer cmd, int source, int depth, int destination, Material material, int materialPass)
	{
		Vector4 value = new Vector4(1f, 1f, 0f, 0f);
		cmd.SetGlobalTexture(BlitBuffer._BlitTexture, source);
		cmd.SetGlobalVector(BlitBuffer._BlitScaleBias, value);
		cmd.SetGlobalFloat(BlitBuffer._BlitMipLevel, 0f);
		if (depth != -1)
		{
			cmd.SetRenderTarget((RenderTargetIdentifier)destination, (RenderTargetIdentifier)depth, 0);
		}
		else
		{
			cmd.SetRenderTarget(destination, 0);
		}
		cmd.DrawProcedural(Matrix4x4.identity, material, materialPass, MeshTopology.Triangles, 3, 1);
		cmd.SetRenderTarget(-1);
	}

	private void DoMaskedColorTransform(CommandBuffer cmd, int source, int dest)
	{
		Blit(cmd, source, -1, dest, m_BlitMaterial, 0);
		cmd.SetGlobalFloat(ShaderConstants._StencilRef, (float)m_MaskedColorTransform.StencilRef.value);
		cmd.SetGlobalVector(ShaderConstants._MaskedColorTransformParams, new Vector4(m_MaskedColorTransform.Brightness.value, m_MaskedColorTransform.Contrast.value + 1f, 0f, 0f));
		Blit(cmd, source, m_Depth.Id, dest, m_Materials.MaskedColorTransform, 0);
	}

	private void DoSaturationOverlay(CommandBuffer cmd, int source, int destination)
	{
		Blit(cmd, source, m_Depth.Id, destination, m_Materials.SaturationOverlay, 0);
	}
}
