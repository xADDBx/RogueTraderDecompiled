using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

[Serializable]
public class PostProcessData : ScriptableObject
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[Reload("Shaders/PostProcessing/LutBuilderLdr.shader", ReloadAttribute.Package.Root)]
		public Shader LutBuilderLdrPS;

		[Reload("Shaders/PostProcessing/LutBuilderHdr.shader", ReloadAttribute.Package.Root)]
		public Shader LutBuilderHdrPS;

		[Reload("Shaders/PostProcessing/StopNaN.shader", ReloadAttribute.Package.Root)]
		public Shader StopNanPS;

		[Reload("Shaders/PostProcessing/SubpixelMorphologicalAntialiasing.shader", ReloadAttribute.Package.Root)]
		public Shader SubpixelMorphologicalAntialiasingPS;

		[Reload("Shaders/PostProcessing/TemporalAA.shader", ReloadAttribute.Package.Root)]
		public Shader TemporalAntialiasingPS;

		[Reload("Shaders/PostProcessing/GaussianDepthOfField.shader", ReloadAttribute.Package.Root)]
		public Shader GaussianDepthOfFieldPS;

		[Reload("Shaders/PostProcessing/BokehDepthOfField.shader", ReloadAttribute.Package.Root)]
		public Shader BokehDepthOfFieldPS;

		[Reload("Shaders/PostProcessing/CameraMotionBlur.shader", ReloadAttribute.Package.Root)]
		public Shader CameraMotionBlurPS;

		[Reload("Shaders/PostProcessing/PaniniProjection.shader", ReloadAttribute.Package.Root)]
		public Shader PaniniProjectionPS;

		[Reload("Shaders/PostProcessing/Bloom.shader", ReloadAttribute.Package.Root)]
		public Shader BloomPS;

		[Reload("Shaders/PostProcessing/BloomEnhanced.shader", ReloadAttribute.Package.Root)]
		public Shader BloomEnhancedPS;

		[Reload("Shaders/PostProcessing/RadialBlur.shader", ReloadAttribute.Package.Root)]
		public Shader RadialBlurPS;

		[Reload("Shaders/PostProcessing/MaskedColorTransform.shader", ReloadAttribute.Package.Root)]
		public Shader MaskedColorTransformPS;

		[Reload("Shaders/PostProcessing/UberPost.shader", ReloadAttribute.Package.Root)]
		public Shader UberPostPS;

		[Reload("Shaders/PostProcessing/FinalPost.shader", ReloadAttribute.Package.Root)]
		public Shader FinalPostPassPS;

		[Reload("Shaders/PostProcessing/Daltonization.shader", ReloadAttribute.Package.Root)]
		public Shader DaltonizationPS;

		[Reload("Shaders/Utils/ScreenSpaceCloudShadows.shader", ReloadAttribute.Package.Root)]
		public Shader ScreenSpaceCloudShadowsShader;

		[Reload("Shaders/Utils/FinalBlit.shader", ReloadAttribute.Package.Root)]
		public Shader FinalBlitPS;
	}

	[Serializable]
	[ReloadGroup]
	public sealed class TextureResources
	{
		[Reload("Shaders/PostProcessing/Textures/BlueNoise16/L/LDR_LLL1_{0}.png", 0, 32, ReloadAttribute.Package.Root)]
		public Texture2D[] BlueNoise16LTex;

		[Reload(new string[] { "Shaders/PostProcessing/Textures/FilmGrain/Thin01.png", "Shaders/PostProcessing/Textures/FilmGrain/Thin02.png", "Shaders/PostProcessing/Textures/FilmGrain/Medium01.png", "Shaders/PostProcessing/Textures/FilmGrain/Medium02.png", "Shaders/PostProcessing/Textures/FilmGrain/Medium03.png", "Shaders/PostProcessing/Textures/FilmGrain/Medium04.png", "Shaders/PostProcessing/Textures/FilmGrain/Medium05.png", "Shaders/PostProcessing/Textures/FilmGrain/Medium06.png", "Shaders/PostProcessing/Textures/FilmGrain/Large01.png", "Shaders/PostProcessing/Textures/FilmGrain/Large02.png" }, ReloadAttribute.Package.Root)]
		public Texture2D[] FilmGrainTex;

		[Reload("Shaders/PostProcessing/Textures/SMAA/AreaTex.tga", ReloadAttribute.Package.Root)]
		public Texture2D SmaaAreaTex;

		[Reload("Shaders/PostProcessing/Textures/SMAA/SearchTex.tga", ReloadAttribute.Package.Root)]
		public Texture2D SmaaSearchTex;
	}

	public ShaderResources Shaders;

	public TextureResources Textures;
}
