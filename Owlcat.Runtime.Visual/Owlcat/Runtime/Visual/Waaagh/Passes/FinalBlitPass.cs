using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

internal sealed class FinalBlitPass : ScriptableRenderPass<FinalBlitPass.PassData>, IDisposable
{
	internal enum IntermediateBlitType
	{
		None,
		Easu,
		Bilinear,
		NearestNeighbour
	}

	internal enum OutputBlitType
	{
		Bilinear,
		NearestNeighbour
	}

	internal sealed class PassData : PassDataBase
	{
		public TextureHandle InputTexture;

		public ColorSpace InputColorSpace;

		public Vector2Int InputViewportSize;

		public TextureHandle IntermediateTexture;

		public TextureHandle OutputTexture;

		public ColorSpace OutputColorSpace;

		public Rect OutputViewport;

		public IntermediateBlitType IntermediateBlitType;

		public OutputBlitType OutputBlitType;

		public Matrix4x4 ViewMatrixToRestore;

		public Matrix4x4 ProjectionMatrixToRestore;

		public bool ApplyRcas;

		public float RcasSharpness;

		public bool ApplyDithering;

		public bool ApplyFilmGrain;

		public FilmGrain FilmGrain;
	}

	private static readonly int _FsrEasuScreenSize = Shader.PropertyToID("_FsrEasuScreenSize");

	private readonly PostProcessData m_PostProcessData;

	private readonly Material m_IntermediateBlitMaterial;

	private readonly Material m_FinalBlitMaterial;

	private readonly Material m_EasuUpscaleMaterial;

	private int m_DitheringTextureIndex;

	public override string Name => "FinalBlitPass";

	public bool ApplyTaaRcas { get; set; }

	public bool ApplyNoiseBasedEffects { get; set; }

	public FinalBlitPass(RenderPassEvent evt, PostProcessData postProcessData, Shader finalBlitShader, Shader easuUpscaleShader)
		: base(evt)
	{
		m_PostProcessData = postProcessData;
		m_IntermediateBlitMaterial = CoreUtils.CreateEngineMaterial(finalBlitShader);
		m_FinalBlitMaterial = CoreUtils.CreateEngineMaterial(finalBlitShader);
		m_EasuUpscaleMaterial = CoreUtils.CreateEngineMaterial(easuUpscaleShader);
	}

	public void Dispose()
	{
		CoreUtils.Destroy(m_IntermediateBlitMaterial);
		CoreUtils.Destroy(m_FinalBlitMaterial);
		CoreUtils.Destroy(m_EasuUpscaleMaterial);
	}

	protected override void Setup(RenderGraphBuilder builder, PassData data, ref RenderingData renderingData)
	{
		builder.AllowPassCulling(value: false);
		data.OutputViewport = renderingData.CameraData.CameraResolveTargetBufferType switch
		{
			CameraResolveTargetType.NonScaled => new Rect(0f, 0f, renderingData.CameraData.FinalTargetViewport.width, renderingData.CameraData.FinalTargetViewport.height), 
			CameraResolveTargetType.Backbuffer => renderingData.CameraData.FinalTargetViewport, 
			_ => throw new InvalidOperationException($"Unexpected camera resolve target buffer type: {renderingData.CameraData.CameraResolveTargetBufferType}"), 
		};
		if (ApplyNoiseBasedEffects)
		{
			data.ApplyDithering = renderingData.CameraData.IsDitheringEnabled;
			data.FilmGrain = VolumeManager.instance.stack.GetComponent<FilmGrain>();
			data.ApplyFilmGrain = data.FilmGrain.IsActive();
		}
		else
		{
			data.ApplyDithering = false;
			data.FilmGrain = null;
			data.ApplyFilmGrain = false;
		}
		if (renderingData.CameraData.CameraRenderTargetBufferType == CameraRenderTargetType.Scaled)
		{
			switch (renderingData.CameraData.ScalingMode)
			{
			case ImageScalingMode.Upscaling:
				switch (renderingData.CameraData.UpscalingFilter)
				{
				case ImageUpscalingFilter.FSR:
					data.ApplyRcas = true;
					data.RcasSharpness = renderingData.CameraData.FsrSharpness;
					data.IntermediateBlitType = IntermediateBlitType.Easu;
					data.OutputBlitType = OutputBlitType.Bilinear;
					break;
				case ImageUpscalingFilter.Point:
					data.ApplyRcas = ApplyTaaRcas;
					data.RcasSharpness = renderingData.CameraData.TemporalAntialiasingSharpness;
					if (data.ApplyRcas || data.ApplyFilmGrain || data.ApplyDithering)
					{
						data.IntermediateBlitType = IntermediateBlitType.NearestNeighbour;
						data.OutputBlitType = OutputBlitType.Bilinear;
					}
					else
					{
						data.IntermediateBlitType = IntermediateBlitType.None;
						data.OutputBlitType = OutputBlitType.NearestNeighbour;
					}
					break;
				default:
					data.ApplyRcas = ApplyTaaRcas;
					data.RcasSharpness = renderingData.CameraData.TemporalAntialiasingSharpness;
					if (data.ApplyRcas || data.ApplyFilmGrain || data.ApplyDithering)
					{
						data.IntermediateBlitType = IntermediateBlitType.Bilinear;
					}
					else
					{
						data.IntermediateBlitType = IntermediateBlitType.None;
					}
					data.OutputBlitType = OutputBlitType.Bilinear;
					break;
				}
				break;
			case ImageScalingMode.Downscaling:
				data.ApplyRcas = ApplyTaaRcas;
				data.RcasSharpness = renderingData.CameraData.TemporalAntialiasingSharpness;
				if (data.ApplyRcas || data.ApplyFilmGrain || data.ApplyDithering)
				{
					data.IntermediateBlitType = IntermediateBlitType.Bilinear;
				}
				else
				{
					data.IntermediateBlitType = IntermediateBlitType.None;
				}
				data.OutputBlitType = OutputBlitType.Bilinear;
				break;
			default:
				data.ApplyRcas = ApplyTaaRcas;
				data.RcasSharpness = renderingData.CameraData.TemporalAntialiasingSharpness;
				data.IntermediateBlitType = IntermediateBlitType.None;
				if (data.ApplyRcas || data.ApplyFilmGrain || data.ApplyDithering)
				{
					data.IntermediateBlitType = IntermediateBlitType.None;
					data.OutputBlitType = OutputBlitType.Bilinear;
				}
				else
				{
					data.IntermediateBlitType = IntermediateBlitType.None;
					data.OutputBlitType = OutputBlitType.NearestNeighbour;
				}
				break;
			}
			data.InputViewportSize = renderingData.CameraData.ScaledCameraTargetViewportSize;
		}
		else
		{
			data.ApplyRcas = ApplyTaaRcas;
			data.RcasSharpness = renderingData.CameraData.TemporalAntialiasingSharpness;
			data.IntermediateBlitType = IntermediateBlitType.None;
			if (data.ApplyRcas || data.ApplyFilmGrain || data.ApplyDithering)
			{
				data.OutputBlitType = OutputBlitType.Bilinear;
			}
			else
			{
				data.OutputBlitType = OutputBlitType.NearestNeighbour;
			}
			data.InputViewportSize = new Vector2Int((int)renderingData.CameraData.FinalTargetViewport.width, (int)renderingData.CameraData.FinalTargetViewport.height);
		}
		if (data.IntermediateBlitType != 0)
		{
			TextureDesc desc = RenderingUtils.CreateTextureDesc("Temp_Intermediate", renderingData.CameraData.CameraTargetDescriptor);
			desc.width = (int)data.OutputViewport.width;
			desc.height = (int)data.OutputViewport.height;
			desc.depthBufferBits = DepthBits.None;
			desc.filterMode = FilterMode.Bilinear;
			desc.wrapMode = TextureWrapMode.Clamp;
			data.IntermediateTexture = builder.CreateTransientTexture(in desc);
		}
		else
		{
			data.IntermediateTexture = TextureHandle.nullHandle;
		}
		data.InputTexture = builder.ReadTexture(in data.Resources.CameraColorBuffer);
		data.OutputTexture = builder.WriteTexture(in data.Resources.CameraResolveColorBuffer);
		data.InputColorSpace = ColorSpace.Gamma;
		if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
		{
			data.OutputColorSpace = ColorSpace.Gamma;
		}
		else
		{
			data.OutputColorSpace = ((renderingData.CameraData.CameraResolveTargetBufferType == CameraResolveTargetType.Backbuffer) ? ColorSpace.Linear : ColorSpace.Gamma);
		}
		data.ViewMatrixToRestore = renderingData.CameraData.Camera.worldToCameraMatrix;
		data.ProjectionMatrixToRestore = renderingData.CameraData.Camera.projectionMatrix;
	}

	protected override void Render(PassData data, RenderGraphContext context)
	{
		BlitToIntermediateTarget(data, context);
		BlitToOutputTarget(data, context);
		context.cmd.SetViewProjectionMatrices(data.ViewMatrixToRestore, data.ProjectionMatrixToRestore);
	}

	private void BlitToIntermediateTarget(PassData data, RenderGraphContext context)
	{
		switch (data.IntermediateBlitType)
		{
		case IntermediateBlitType.Easu:
			BlitEasuUpscale(context.cmd, data.InputTexture, data.IntermediateTexture, data.InputViewportSize, data.OutputViewport.size);
			break;
		case IntermediateBlitType.Bilinear:
			FinalBlitter.Blit(context.cmd, inputTexture: data.InputTexture, outputTexture: data.IntermediateTexture, outputViewport: data.OutputViewport, material: m_IntermediateBlitMaterial, inputColorSpace: ColorSpace.Gamma, outputColorSpace: ColorSpace.Linear, samplerType: FinalBlitter.SamplerType.Bilinear);
			break;
		case IntermediateBlitType.NearestNeighbour:
			FinalBlitter.Blit(context.cmd, inputTexture: data.InputTexture, outputTexture: data.IntermediateTexture, outputViewport: data.OutputViewport, material: m_IntermediateBlitMaterial, inputColorSpace: ColorSpace.Gamma, outputColorSpace: ColorSpace.Linear, samplerType: FinalBlitter.SamplerType.NearestNeighbour);
			break;
		}
	}

	private void BlitToOutputTarget(PassData data, RenderGraphContext context)
	{
		TextureHandle textureHandle;
		ColorSpace colorSpace;
		if (data.IntermediateBlitType != 0)
		{
			textureHandle = data.IntermediateTexture;
			colorSpace = ColorSpace.Linear;
		}
		else
		{
			textureHandle = data.InputTexture;
			colorSpace = data.InputColorSpace;
		}
		FinalBlitter.SamplerType samplerType = ((data.OutputBlitType == OutputBlitType.NearestNeighbour) ? FinalBlitter.SamplerType.NearestNeighbour : FinalBlitter.SamplerType.Bilinear);
		FinalBlitter.SamplerType samplerType2 = samplerType;
		FinalBlitter.Blit(context.cmd, samplerType: samplerType2, inputTexture: textureHandle, inputColorSpace: colorSpace, outputTexture: data.OutputTexture, outputColorSpace: data.OutputColorSpace, outputViewport: data.OutputViewport, applyRcas: data.ApplyRcas, applyDithering: data.ApplyDithering, applyFilmGrain: data.ApplyFilmGrain, rcasSharpness: data.RcasSharpness, filmGrain: data.FilmGrain, material: m_FinalBlitMaterial, postProcessData: m_PostProcessData, ditheringTextureIndex: ref m_DitheringTextureIndex);
	}

	private void BlitEasuUpscale(CommandBuffer cmd, TextureHandle inputTexture, TextureHandle outputTexture, Vector2 inputViewportSize, Vector2 outputViewportSize)
	{
		FSRUtils.SetEasuConstants(cmd, inputViewportSize, inputViewportSize, outputViewportSize);
		cmd.SetGlobalVector(_FsrEasuScreenSize, outputViewportSize);
		cmd.SetGlobalTexture(ShaderPropertyId._BlitTex, inputTexture);
		cmd.SetRenderTarget(outputTexture);
		cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, m_EasuUpscaleMaterial);
	}
}
