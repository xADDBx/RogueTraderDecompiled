using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

internal static class FinalBlitter
{
	public enum SamplerType
	{
		Bilinear,
		NearestNeighbour
	}

	private const float kMaxSharpnessStops = 2.5f;

	private static readonly int _FsrRcasScreenSize = Shader.PropertyToID("_FsrRcasScreenSize");

	private static readonly GlobalKeyword _FILTER_POINT = GlobalKeyword.Create("_FILTER_POINT");

	private static readonly GlobalKeyword _RCAS = GlobalKeyword.Create("_RCAS");

	private static readonly GlobalKeyword _DITHERING = GlobalKeyword.Create("_DITHERING");

	private static readonly GlobalKeyword _FILM_GRAIN = GlobalKeyword.Create("_FILM_GRAIN");

	private static readonly GlobalKeyword _INPUT_COLORSPACE_GAMMA = GlobalKeyword.Create("_INPUT_COLORSPACE_GAMMA");

	private static readonly GlobalKeyword _OUTPUT_COLORSPACE_GAMMA = GlobalKeyword.Create("_OUTPUT_COLORSPACE_GAMMA");

	public static void Blit(CommandBuffer cmd, Material material, TextureHandle inputTexture, TextureHandle outputTexture, ColorSpace inputColorSpace, ColorSpace outputColorSpace, Rect outputViewport, SamplerType samplerType)
	{
		Blit(cmd, material, inputTexture, outputTexture, inputColorSpace, outputColorSpace, outputViewport, samplerType, applyRcas: false, applyDithering: false, applyFilmGrain: false);
	}

	public static void Blit(CommandBuffer cmd, Material material, TextureHandle inputTexture, TextureHandle outputTexture, ColorSpace inputColorSpace, ColorSpace outputColorSpace, Rect outputViewport, bool applyRcas, bool applyDithering, bool applyFilmGrain, float rcasSharpness, FilmGrain filmGrain, PostProcessData postProcessData, SamplerType samplerType, ref int ditheringTextureIndex)
	{
		if (applyRcas)
		{
			FSRUtils.SetRcasConstantsLinear(cmd, rcasSharpness);
			cmd.SetGlobalVector(_FsrRcasScreenSize, outputViewport.size);
		}
		if (applyDithering)
		{
			ditheringTextureIndex = PostProcessUtils.ConfigureDithering(postProcessData, ditheringTextureIndex, (int)outputViewport.width, (int)outputViewport.height, material);
		}
		if (applyFilmGrain)
		{
			PostProcessUtils.ConfigureFilmGrain(postProcessData, filmGrain, (int)outputViewport.width, (int)outputViewport.height, material);
		}
		Blit(cmd, material, inputTexture, outputTexture, inputColorSpace, outputColorSpace, outputViewport, samplerType, applyRcas, applyDithering, applyFilmGrain);
	}

	private static void Blit(CommandBuffer cmd, Material material, TextureHandle inputTexture, TextureHandle outputTexture, ColorSpace inputColorSpace, ColorSpace outputColorSpace, Rect outputViewport, SamplerType samplerType, bool applyRcas, bool applyDithering, bool applyFilmGrain)
	{
		cmd.SetKeyword(in _FILTER_POINT, samplerType == SamplerType.NearestNeighbour);
		cmd.SetKeyword(in _INPUT_COLORSPACE_GAMMA, inputColorSpace == ColorSpace.Gamma);
		cmd.SetKeyword(in _OUTPUT_COLORSPACE_GAMMA, outputColorSpace == ColorSpace.Gamma);
		cmd.SetKeyword(in _RCAS, applyRcas);
		cmd.SetKeyword(in _DITHERING, applyDithering);
		cmd.SetKeyword(in _FILM_GRAIN, applyFilmGrain);
		cmd.SetGlobalTexture(ShaderPropertyId._BlitTex, inputTexture);
		cmd.SetRenderTarget(outputTexture);
		cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		cmd.SetViewport(outputViewport);
		cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, material, 0, 0);
	}
}
