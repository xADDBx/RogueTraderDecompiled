using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting.Passes;

public class HighlighterPass : ScriptableRenderPass<HighlighterPassData>
{
	private HighlightingFeature m_HighlightFeature;

	private Material m_HighlighterMat;

	private Material m_BlurMat;

	private Material m_CutMat;

	private Material m_CompositeMat;

	public override string Name => "HighlighterPass";

	public HighlighterPass(RenderPassEvent evt, HighlightingFeature feature, Material highlightMat, Material blurMat, Material cutMat, Material compositeMat)
		: base(evt)
	{
		m_HighlightFeature = feature;
		m_HighlighterMat = highlightMat;
		m_BlurMat = blurMat;
		m_CutMat = cutMat;
		m_CompositeMat = compositeMat;
	}

	protected override void Setup(RenderGraphBuilder builder, HighlighterPassData data, ref RenderingData renderingData)
	{
		data.Feature = m_HighlightFeature;
		data.HighlighterMaterial = m_HighlighterMat;
		data.BlurMaterial = m_BlurMat;
		data.CutMaterial = m_CutMat;
		data.CompositeMaterial = m_CompositeMat;
		data.CameraColorRT = builder.WriteTexture(in data.Resources.CameraColorBuffer);
		TextureDesc desc = RenderingUtils.CreateTextureDesc("HighlightRT", renderingData.CameraData.CameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		data.HighlightRT = builder.CreateTransientTexture(in desc);
		int num = Mathf.Max(1, (int)m_HighlightFeature.DownsampleFactor);
		TextureDesc desc2 = desc;
		desc2.width = desc.width / num;
		desc2.height = desc.height / num;
		desc2.name = "Blur1RT";
		data.Blur1RT = builder.CreateTransientTexture(in desc2);
		desc2.name = "Blur2RT";
		data.Blur2RT = builder.CreateTransientTexture(in desc2);
		data.ZTestMode = m_HighlightFeature.ZTest;
		switch (data.ZTestMode)
		{
		case HighlightingFeature.ZTestMode.SceneBuffer:
			data.DepthBuffer = builder.ReadWriteTexture(in data.Resources.CameraDepthBuffer);
			break;
		case HighlightingFeature.ZTestMode.EmptyBuffer:
		{
			TextureDesc desc3 = desc;
			desc3.name = "HighlightDepthRT";
			desc3.colorFormat = GraphicsFormat.D24_UNorm_S8_UInt;
			desc3.depthBufferBits = DepthBits.Depth32;
			data.DepthBuffer = builder.CreateTransientTexture(in desc3);
			break;
		}
		}
		data.ParticlesShader = m_HighlightFeature.Shaders.ParticlesShader;
	}

	protected override void Render(HighlighterPassData data, RenderGraphContext context)
	{
		if (data.ZTestMode == HighlightingFeature.ZTestMode.None)
		{
			context.cmd.SetRenderTarget(data.HighlightRT);
		}
		else
		{
			context.cmd.SetRenderTarget(data.HighlightRT, data.DepthBuffer);
		}
		context.cmd.ClearRenderTarget(data.Feature.ZTest != HighlightingFeature.ZTestMode.SceneBuffer, clearColor: true, Color.clear);
		context.cmd.SetGlobalFloat(HighlightConstantBuffer._ZTest, (data.Feature.ZTest == HighlightingFeature.ZTestMode.SceneBuffer) ? 3 : 4);
		context.cmd.SetGlobalFloat(HighlightConstantBuffer._ZWrite, (data.Feature.ZTest == HighlightingFeature.ZTestMode.EmptyBuffer) ? 1 : 0);
		List<Material> value;
		using (CollectionPool<List<Material>, Material>.Get(out value))
		{
			List<HighlightingFeature.RendererInfo> rendererInfos = data.Feature.RendererInfos;
			int i = 0;
			for (int count = rendererInfos.Count; i < count; i++)
			{
				if (!data.Feature.IsRendererVisible(i))
				{
					continue;
				}
				HighlightingFeature.RendererInfo rendererInfo = rendererInfos[i];
				if (rendererInfo.renderer == null || !rendererInfo.renderer.enabled || !rendererInfo.renderer.gameObject.activeInHierarchy)
				{
					continue;
				}
				rendererInfo.renderer.GetSharedMaterials(value);
				if (value.Count == 0)
				{
					continue;
				}
				context.cmd.SetGlobalColor(HighlightConstantBuffer._Color, rendererInfo.highlighter.CurrentColor);
				int num = ((rendererInfo.expectedMaterialsCount > 0) ? Mathf.Min(value.Count, rendererInfo.expectedMaterialsCount) : value.Count);
				for (int j = 0; j < num; j++)
				{
					Material material = value[j];
					if (material == null)
					{
						continue;
					}
					bool state = false;
					Texture texture = null;
					float value2 = 0f;
					CullMode cullMode = CullMode.Back;
					if (material.HasProperty(HighlightConstantBuffer._Alphatest))
					{
						state = material.GetFloat(HighlightConstantBuffer._Alphatest) > 0f;
					}
					if (material.shader == data.ParticlesShader)
					{
						state = true;
					}
					if (material.HasProperty(HighlightConstantBuffer._CullMode))
					{
						cullMode = (CullMode)material.GetFloat(HighlightConstantBuffer._CullMode);
					}
					if (material.HasProperty(HighlightConstantBuffer._BaseMap))
					{
						texture = material.GetTexture(HighlightConstantBuffer._BaseMap);
					}
					if (material.HasProperty(HighlightConstantBuffer._Cutoff))
					{
						value2 = material.GetFloat(HighlightConstantBuffer._Cutoff);
					}
					context.cmd.SetGlobalFloat(HighlightConstantBuffer._CullMode, (float)cullMode);
					if (texture != null)
					{
						context.cmd.SetGlobalTexture(HighlightConstantBuffer._BaseMap, texture);
					}
					else
					{
						context.cmd.SetGlobalTexture(HighlightConstantBuffer._BaseMap, context.defaultResources.whiteTexture);
					}
					context.cmd.SetGlobalFloat(HighlightConstantBuffer._Cutoff, value2);
					CoreUtils.SetKeyword(context.cmd, HighlightKeywords._ALPHATEST_ON, state);
					bool flag = material.IsKeywordEnabled(HighlightKeywords.VAT_ENABLED);
					CoreUtils.SetKeyword(context.cmd, HighlightKeywords.VAT_ENABLED, flag);
					if (flag)
					{
						bool flag2 = material.IsKeywordEnabled(HighlightKeywords._VAT_ROTATIONMAP);
						CoreUtils.SetKeyword(context.cmd, HighlightKeywords._VAT_ROTATIONMAP, flag2);
						if (flag2)
						{
							SetTexture(HighlightConstantBuffer._RotVatMap, context.cmd, material);
						}
						SetTexture(HighlightConstantBuffer._PosVatMap, context.cmd, material);
						SetFloat(HighlightConstantBuffer._VatCurrentFrame, context.cmd, material);
						SetFloat(HighlightConstantBuffer._VatLerp, context.cmd, material);
						SetFloat(HighlightConstantBuffer._VatNumOfFrames, context.cmd, material);
						SetFloat(HighlightConstantBuffer._VatPivMax, context.cmd, material);
						SetFloat(HighlightConstantBuffer._VatPivMin, context.cmd, material);
						SetFloat(HighlightConstantBuffer._VatPosMax, context.cmd, material);
						SetFloat(HighlightConstantBuffer._VatPosMin, context.cmd, material);
						SetFloat(HighlightConstantBuffer._VatType, context.cmd, material);
					}
					bool state2 = material.IsKeywordEnabled(HighlightKeywords.PBD_MESH);
					CoreUtils.SetKeyword(context.cmd, HighlightKeywords.PBD_MESH, state2);
					bool state3 = material.IsKeywordEnabled(HighlightKeywords.PBD_SKINNING);
					CoreUtils.SetKeyword(context.cmd, HighlightKeywords.PBD_SKINNING, state3);
					context.cmd.DrawRenderer(rendererInfo.renderer, data.HighlighterMaterial, j, 0);
				}
			}
		}
		context.cmd.Blit(data.HighlightRT, data.Blur1RT);
		CoreUtils.SetKeyword(data.BlurMaterial, HighlightKeywords.STRAIGHT_DIRECTIONS, data.Feature.BlurDirectons == HighlightingFeature.BlurDirections.Straight);
		CoreUtils.SetKeyword(data.BlurMaterial, HighlightKeywords.ALL_DIRECTIONS, data.Feature.BlurDirectons == HighlightingFeature.BlurDirections.All);
		bool flag3 = true;
		for (int k = 0; k < data.Feature.BlurIterations; k++)
		{
			float value3 = data.Feature.BlurMinSpread + data.Feature.BlurSpread * (float)k;
			context.cmd.SetGlobalFloat(HighlightConstantBuffer._HighlightingBlurOffset, value3);
			if (flag3)
			{
				context.cmd.Blit(data.Blur1RT, data.Blur2RT, data.BlurMaterial);
			}
			else
			{
				context.cmd.Blit(data.Blur2RT, data.Blur1RT, data.BlurMaterial);
			}
			flag3 = !flag3;
		}
		context.cmd.Blit(flag3 ? data.Blur1RT : data.Blur2RT, data.HighlightRT, data.CutMaterial);
		context.cmd.Blit(data.HighlightRT, data.CameraColorRT, data.CompositeMaterial);
	}

	private void SetFloat(int property, CommandBuffer cmd, Material mat)
	{
		if (mat.HasProperty(property))
		{
			cmd.SetGlobalFloat(property, mat.GetFloat(property));
		}
	}

	private void SetTexture(int property, CommandBuffer cmd, Material mat)
	{
		if (mat.HasProperty(property))
		{
			cmd.SetGlobalTexture(property, mat.GetTexture(property));
		}
	}
}
