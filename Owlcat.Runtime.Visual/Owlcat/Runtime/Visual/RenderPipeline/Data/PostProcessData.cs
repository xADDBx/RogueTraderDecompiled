using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Data;

public class PostProcessData : ScriptableObject
{
	[Serializable]
	public sealed class ShaderResources
	{
		public Shader StopNanPS;

		public Shader SubpixelMorphologicalAntialiasingPS;

		public Shader GaussianDepthOfFieldPS;

		public Shader BokehDepthOfFieldPS;

		public Shader CameraMotionBlurPS;

		public Shader PaniniProjectionPS;

		public Shader LutBuilderLdrPS;

		public Shader LutBuilderHdrPS;

		public Shader BloomPS;

		public Shader BloomEnhancedPS;

		public Shader RadialBlurPS;

		public Shader SaturationOverlayPS;

		public Shader MaskedColorTransformPS;

		public Shader UberPostPS;

		public Shader FinalPostPassPS;

		public Shader DaltonizationPS;
	}

	[Serializable]
	public sealed class TextureResources
	{
		public Texture2D[] BlueNoise16LTex;

		public Texture2D[] FilmGrainTex;

		public Texture2D SmaaAreaTex;

		public Texture2D SmaaSearchTex;
	}

	public ShaderResources Shaders;

	public TextureResources Textures;
}
