using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class BeforeTransparentPostProcessPass : ScriptableRenderPass
{
	private class MaterialLibrary
	{
		private List<Material> m_Materials;

		public readonly Material ScreenSpaceCloudShadows;

		public readonly Material MaskedColorTransform;

		public MaterialLibrary(PostProcessData data)
		{
			m_Materials = new List<Material>();
			ScreenSpaceCloudShadows = Load(data.Shaders.ScreenSpaceCloudShadowsShader);
			MaskedColorTransform = Load(data.Shaders.MaskedColorTransformPS);
		}

		private Material Load(Shader shader)
		{
			if (shader == null)
			{
				UnityEngine.Debug.LogErrorFormat("Missing shader. " + GetType().DeclaringType.Name + " render pass will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			Material material = CoreUtils.CreateEngineMaterial(shader);
			m_Materials.Add(material);
			return material;
		}

		public void Dispose()
		{
			foreach (Material material in m_Materials)
			{
				CoreUtils.Destroy(material);
			}
			m_Materials.Clear();
		}
	}

	private static class ShaderConstants
	{
		public static readonly int _MaskedColorTransformParams = Shader.PropertyToID("_MaskedColorTransformParams");

		public static readonly int _Texture0 = Shader.PropertyToID("_Texture0");

		public static readonly int _Texture1 = Shader.PropertyToID("_Texture1");

		public static readonly int _Texture0ScaleBias = Shader.PropertyToID("_Texture0ScaleBias");

		public static readonly int _Texture1ScaleBias = Shader.PropertyToID("_Texture1ScaleBias");

		public static readonly int _Texture0Color = Shader.PropertyToID("_Texture0Color");

		public static readonly int _Texture1Color = Shader.PropertyToID("_Texture1Color");

		public static readonly int _Intensity = Shader.PropertyToID("_Intensity");
	}

	private class PostProcessPassDataBase : PassDataBase
	{
		public TextureHandle Source;

		public TextureHandle Destination;

		public MaterialLibrary Materials;
	}

	private class SSCSPassData : PostProcessPassDataBase
	{
		public TextureHandle CameraDepthRT;

		public TextureHandle CameraBakedGIRT;

		public Texture CloudTexture0;

		public Texture CloudTexture1;

		public Vector4 Tex0ScaleBias;

		public Vector4 Tex1ScaleBias;

		public Vector4 Tex0Color;

		public Vector4 Tex1Color;

		public float Intensity;
	}

	private class MaskedColorTransformPassData : PostProcessPassDataBase
	{
		public TextureHandle CameraDepthRT;

		public Material BlitMaterial;

		public float StencilRef;

		public Vector4 MaskedColorTransformParams;
	}

	private PostProcessData m_Data;

	private MaterialLibrary m_Materials;

	private Material m_BlitMaterial;

	private ScreenSpaceCloudShadows m_Sscs;

	private MaskedColorTransform m_MaskedColorTransform;

	private Vector2 m_CloudScroll0;

	private Vector2 m_CloudScroll1;

	public override string Name => "BeforeTransparentPostProcessPass";

	public BeforeTransparentPostProcessPass(RenderPassEvent evt, PostProcessData data, Material blitMaterial)
		: base(evt)
	{
		m_Materials = new MaterialLibrary(data);
		m_BlitMaterial = blitMaterial;
	}

	protected override void RecordRenderGraph(ref RenderingData renderingData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_Sscs = stack.GetComponent<ScreenSpaceCloudShadows>();
		m_MaskedColorTransform = stack.GetComponent<MaskedColorTransform>();
		RenderGraph renderGraph = renderingData.RenderGraph;
		ProfilingSampler sampler = ProfilingSampler.Get(WaaaghProfileId.RenderBeforeTransparentPostProcess);
		renderGraph.BeginProfilingSampler(sampler);
		TextureDesc desc = RenderingUtils.CreateTextureDesc("BeforeTransparentPostProcessRT", renderingData.CameraData.CameraTargetDescriptor);
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		TextureHandle source = renderingData.CameraData.Renderer.RenderGraphResources.CameraColorBuffer;
		TextureHandle destination = TextureHandle.nullHandle;
		if (m_Sscs.IsActive())
		{
			DoSSCS(ref renderingData, GetSource(), GetDestination());
		}
		if (m_MaskedColorTransform.IsActive())
		{
			DoMaskedColorTransform(ref renderingData, GetSource(), GetDestination());
			Swap();
		}
		PostProcessPassDataBase passData2;
		RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PostProcessPassDataBase>("RenderBeforeTransparentPostProcess.FinalBlit", out passData2);
		try
		{
			PostProcessPassDataBase postProcessPassDataBase = passData2;
			TextureHandle input = GetSource();
			postProcessPassDataBase.Source = renderGraphBuilder.ReadTexture(in input);
			passData2.Destination = renderGraphBuilder.WriteTexture(in renderingData.CameraData.Renderer.RenderGraphResources.CameraColorBuffer);
			renderGraphBuilder.SetRenderFunc(delegate(PostProcessPassDataBase passData, RenderGraphContext context)
			{
				RenderTexture renderTexture = passData.Source;
				RenderTexture renderTexture2 = passData.Destination;
				if (renderTexture != renderTexture2)
				{
					context.cmd.Blit(passData.Source, passData.Destination);
				}
			});
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
		renderGraph.EndProfilingSampler(sampler);
		TextureHandle GetDestination()
		{
			if (!destination.IsValid())
			{
				destination = renderGraph.CreateTexture(in desc);
			}
			return destination;
		}
		TextureHandle GetSource()
		{
			return source;
		}
		void Swap()
		{
			CoreUtils.Swap(ref source, ref destination);
		}
	}

	private void DoSSCS(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		SSCSPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<SSCSPassData>("RenderBeforeTransparentPostProcess.SSCS", out passData2);
		passData2.Source = renderGraphBuilder.ReadWriteTexture(in source);
		passData2.Destination = renderGraphBuilder.ReadWriteTexture(in destination);
		passData2.Materials = m_Materials;
		RenderGraphResources renderGraphResources = renderingData.CameraData.Renderer.RenderGraphResources;
		passData2.CameraDepthRT = renderGraphBuilder.ReadTexture(in renderGraphResources.CameraDepthCopyRT);
		passData2.CameraBakedGIRT = renderGraphBuilder.ReadTexture(in renderGraphResources.CameraBakedGIRT);
		passData2.CloudTexture0 = m_Sscs.Texture0.value;
		passData2.CloudTexture1 = m_Sscs.Texture1.value;
		Vector2 value = m_Sscs.Texture0Tiling.value;
		m_CloudScroll0 += m_Sscs.Texture0ScrollSpeed.value * Time.deltaTime;
		passData2.Tex0ScaleBias = new Vector4(value.x, value.y, m_CloudScroll0.x, m_CloudScroll0.y);
		passData2.Tex0Color = m_Sscs.Texture0Color.value;
		value = m_Sscs.Texture1Tiling.value;
		m_CloudScroll1 += m_Sscs.Texture1ScrollSpeed.value * Time.deltaTime;
		passData2.Tex1ScaleBias = new Vector4(value.x, value.y, m_CloudScroll1.x, m_CloudScroll1.y);
		passData2.Tex1Color = m_Sscs.Texture1Color.value;
		passData2.Intensity = m_Sscs.Intensity.value;
		renderGraphBuilder.SetRenderFunc(delegate(SSCSPassData passData, RenderGraphContext context)
		{
			context.cmd.SetGlobalTexture(ShaderConstants._Texture0, passData.CloudTexture0);
			context.cmd.SetGlobalTexture(ShaderConstants._Texture1, passData.CloudTexture1);
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, passData.CameraDepthRT);
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraBakedGIRT, passData.CameraBakedGIRT);
			context.cmd.SetGlobalVector(ShaderConstants._Texture0ScaleBias, passData.Tex0ScaleBias);
			context.cmd.SetGlobalVector(ShaderConstants._Texture0Color, passData.Tex0Color);
			context.cmd.SetGlobalVector(ShaderConstants._Texture1ScaleBias, passData.Tex1ScaleBias);
			context.cmd.SetGlobalVector(ShaderConstants._Texture1Color, passData.Tex1Color);
			context.cmd.SetGlobalFloat(ShaderConstants._Intensity, passData.Intensity);
			context.cmd.SetRenderTarget(passData.Source);
			context.cmd.DrawProcedural(Matrix4x4.identity, passData.Materials.ScreenSpaceCloudShadows, 0, MeshTopology.Triangles, 3);
		});
	}

	private void DoMaskedColorTransform(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		MaskedColorTransformPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<MaskedColorTransformPassData>("RenderBeforeTransparentPostProcess.MaskedColorTrasformPass", out passData2);
		passData2.CameraDepthRT = renderGraphBuilder.ReadTexture(in renderingData.CameraData.Renderer.RenderGraphResources.CameraDepthBuffer);
		passData2.Source = renderGraphBuilder.ReadWriteTexture(in source);
		passData2.Destination = renderGraphBuilder.ReadWriteTexture(in destination);
		passData2.Materials = m_Materials;
		passData2.BlitMaterial = m_BlitMaterial;
		passData2.StencilRef = (float)m_MaskedColorTransform.StencilRef.value;
		passData2.MaskedColorTransformParams = new Vector4(m_MaskedColorTransform.Brightness.value, m_MaskedColorTransform.Contrast.value + 1f, 0f, 0f);
		renderGraphBuilder.SetRenderFunc(delegate(MaskedColorTransformPassData passData, RenderGraphContext context)
		{
			Vector4 value = new Vector4(1f, 1f, 0f, 0f);
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, passData.Source);
			context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
			context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
			context.cmd.SetRenderTarget(passData.Destination, passData.CameraDepthRT);
			context.cmd.DrawProcedural(Matrix4x4.identity, passData.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
			context.cmd.SetGlobalFloat(ShaderPropertyId._StencilRef, passData.StencilRef);
			context.cmd.SetGlobalVector(ShaderConstants._MaskedColorTransformParams, passData.MaskedColorTransformParams);
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, passData.Source);
			context.cmd.SetRenderTarget(passData.Destination, passData.CameraDepthRT);
			context.cmd.DrawProcedural(Matrix4x4.identity, passData.Materials.MaskedColorTransform, 0, MeshTopology.Triangles, 3, 1);
		});
	}

	internal void Cleanup()
	{
		m_Materials.Dispose();
	}
}
