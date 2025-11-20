using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.CustomPostProcess;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess.Passes;

public class CustomPostProcessPass : ScriptableRenderPass
{
	private class MaterialLibrary
	{
		private Dictionary<CustomPostProcessEffectPass, Material> m_PassMaterialMap = new Dictionary<CustomPostProcessEffectPass, Material>();

		public MaterialLibrary(List<CustomPostProcessEffect> effects)
		{
			foreach (CustomPostProcessEffect effect in effects)
			{
				foreach (CustomPostProcessEffectPass pass in effect.Passes)
				{
					if (pass.Shader != null)
					{
						m_PassMaterialMap.Add(pass, CoreUtils.CreateEngineMaterial(pass.Shader));
					}
				}
			}
		}

		public void Dispose()
		{
			foreach (KeyValuePair<CustomPostProcessEffectPass, Material> item in m_PassMaterialMap)
			{
				CoreUtils.Destroy(item.Value);
			}
			m_PassMaterialMap.Clear();
		}

		public Material GetMaterial(CustomPostProcessEffectPass pass)
		{
			m_PassMaterialMap.TryGetValue(pass, out var value);
			return value;
		}
	}

	private class CustomPostProcessPassData : PassDataBase
	{
		public TextureHandle Destination;

		public TextureHandle Source;

		public Material Material;

		public bool DrawProcedural;
	}

	public string m_Name;

	private readonly ProfilingSampler m_ProfilingSampler;

	private readonly List<ProfilingSampler> m_EffectsProfilingSamplers;

	private List<CustomPostProcessEffect> m_Effects;

	private readonly Dictionary<Camera, MaterialLibrary> m_MaterialLibraryCache = new Dictionary<Camera, MaterialLibrary>();

	public List<CustomPostProcessEffect> Effects => m_Effects;

	public override string Name => m_Name;

	public CustomPostProcessPass(RenderPassEvent evt, List<CustomPostProcessEffect> effects)
		: base(evt)
	{
		m_Effects = effects;
		m_Name = string.Format("{0}.{1}", "CustomPostProcessPass", evt);
		m_ProfilingSampler = new ProfilingSampler(m_Name);
		m_EffectsProfilingSamplers = new List<ProfilingSampler>(effects.Count);
		for (int i = 0; i < m_Effects.Count; i++)
		{
			m_EffectsProfilingSamplers.Add(new ProfilingSampler(string.IsNullOrEmpty(m_Effects[i].Name) ? m_Effects[i].name : m_Effects[i].Name));
		}
	}

	protected override void RecordRenderGraph(ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		RenderGraph renderGraph = renderingData.RenderGraph;
		MaterialLibrary materialsForCamera = GetMaterialsForCamera(cameraData.Camera);
		CustomPostProcessOverride component = VolumeManager.instance.stack.GetComponent<CustomPostProcessOverride>();
		renderGraph.BeginProfilingSampler(m_ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\Passes\\CustomPostProcessPass.cs", 86);
		TextureDesc desc = RenderingUtils.CreateTextureDesc("CameraAfterPostProcessRT", cameraData.CameraTargetDescriptor);
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		TextureHandle source = renderingData.CameraData.Renderer.RenderGraphResources.CameraColorBuffer;
		TextureHandle destination = renderGraph.CreateTexture(in desc);
		for (int i = 0; i < m_Effects.Count; i++)
		{
			CustomPostProcessEffect customPostProcessEffect = m_Effects[i];
			if (!component.IsEffectActive(customPostProcessEffect))
			{
				continue;
			}
			renderGraph.BeginProfilingSampler(m_EffectsProfilingSamplers[i], ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\Passes\\CustomPostProcessPass.cs", 121);
			for (int j = 0; j < customPostProcessEffect.Passes.Count; j++)
			{
				CustomPostProcessEffectPass customPostProcessEffectPass = customPostProcessEffect.Passes[j];
				if (customPostProcessEffectPass.Shader == null)
				{
					continue;
				}
				CustomPostProcessPassData passData2;
				RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<CustomPostProcessPassData>(customPostProcessEffectPass.Name, out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\Passes\\CustomPostProcessPass.cs", 131);
				try
				{
					CustomPostProcessPassData customPostProcessPassData = passData2;
					TextureHandle input = GetSource();
					customPostProcessPassData.Source = renderGraphBuilder.ReadTexture(in input);
					CustomPostProcessPassData customPostProcessPassData2 = passData2;
					input = GetDestination();
					customPostProcessPassData2.Destination = renderGraphBuilder.WriteTexture(in input);
					passData2.Material = materialsForCamera.GetMaterial(customPostProcessEffectPass);
					passData2.DrawProcedural = true;
					UpdateMaterialProperties(customPostProcessEffect, customPostProcessEffectPass, passData2.Material, component);
					renderGraphBuilder.SetRenderFunc(delegate(CustomPostProcessPassData data, RenderGraphContext context)
					{
						context.cmd.SetGlobalTexture(ShaderPropertyId._CustomPostProcessInput, data.Source);
						context.cmd.SetRenderTarget(data.Destination);
						if (data.DrawProcedural)
						{
							context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
						}
						else
						{
							context.cmd.Blit(data.Source, data.Destination, data.Material, 0);
						}
					});
				}
				finally
				{
					((IDisposable)renderGraphBuilder).Dispose();
				}
				Swap();
			}
			renderGraph.EndProfilingSampler(m_EffectsProfilingSamplers[i], ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\Passes\\CustomPostProcessPass.cs", 157);
		}
		CustomPostProcessPassData passData3;
		RenderGraphBuilder renderGraphBuilder2 = renderGraph.AddRenderPass<CustomPostProcessPassData>("Final Blit", out passData3, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\Passes\\CustomPostProcessPass.cs", 161);
		try
		{
			CustomPostProcessPassData customPostProcessPassData3 = passData3;
			TextureHandle input = GetSource();
			customPostProcessPassData3.Source = renderGraphBuilder2.ReadTexture(in input);
			passData3.Destination = renderGraphBuilder2.WriteTexture(in renderingData.CameraData.Renderer.RenderGraphResources.CameraColorBuffer);
			renderGraphBuilder2.SetRenderFunc(delegate(CustomPostProcessPassData passData, RenderGraphContext context)
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
			((IDisposable)renderGraphBuilder2).Dispose();
		}
		renderGraph.EndProfilingSampler(m_ProfilingSampler, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\RendererFeatures\\CustomPostProcess\\Passes\\CustomPostProcessPass.cs", 178);
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

	private void UpdateMaterialProperties(CustomPostProcessEffect effect, CustomPostProcessEffectPass effectPass, Material material, CustomPostProcessOverride settings)
	{
		settings.ApplyPropertiesOverride(effect, effectPass, material);
	}

	private MaterialLibrary GetMaterialsForCamera(Camera camera)
	{
		if (!m_MaterialLibraryCache.TryGetValue(camera, out var value))
		{
			value = new MaterialLibrary(m_Effects);
			m_MaterialLibraryCache.Add(camera, value);
		}
		return value;
	}

	internal void Cleanup()
	{
		foreach (KeyValuePair<Camera, MaterialLibrary> item in m_MaterialLibraryCache)
		{
			item.Value.Dispose();
		}
	}

	internal void Validate()
	{
		List<Camera> value;
		using (CollectionPool<List<Camera>, Camera>.Get(out value))
		{
			foreach (Camera key in m_MaterialLibraryCache.Keys)
			{
				if (key == null)
				{
					value.Add(key);
				}
			}
			foreach (Camera item in value)
			{
				if (m_MaterialLibraryCache.Remove(item, out var value2))
				{
					value2.Dispose();
				}
			}
		}
	}

	internal bool ValidateMaterial(Camera camera, CustomPostProcessEffectPass effectPass)
	{
		Material material = GetMaterialsForCamera(camera).GetMaterial(effectPass);
		if (material == null)
		{
			return false;
		}
		if (material.shader != effectPass.Shader)
		{
			return false;
		}
		return true;
	}
}
