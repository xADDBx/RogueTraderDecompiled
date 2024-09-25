using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ScreenSpaceReflectionsPassData : PassDataBase
{
	public ComputeShader SsrShader;

	public int TraceHiZKernel;

	public int TraceScreenSpaceKernel;

	public Material SsrResolveMaterial;

	public int SsrResolvePass;

	public int SsrBlurPass;

	public int SsrCompositeSsrPass;

	public Material BlitMaterial;

	public TextureHandle SsrHitPointRT;

	public TextureHandle SsrRT;

	public TextureHandle SsrPyramidRT;

	public TextureHandle TempRT0;

	public TextureHandle CameraDepthRT;

	public TextureHandle CameraDepthPyramidRT;

	public TextureHandle CameraHistoryColorRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraTranslucencyRT;

	public TextureHandle MotionVectorsRT;

	public int MinDepthLevel;

	public Camera Camera;

	public Vector4 SsrScreenSize;

	public int2 TextureSize;

	public bool HighlightSupression;

	public int2 ScreenSize;

	public bool UseMotionVectorsForReprojection;

	public bool BlurEnabled;

	public Vector4 RoughnessRemap;

	public int MaxRaySteps;

	internal float ThicknessScale;

	internal float ThicknessBias;

	internal float ObjectThickness;

	internal float MaxRoughness;

	internal float MaxDistance;

	internal float ScreenSpaceStepSize;

	internal float FresnelPower;

	internal float RoughnessFadeStart;
}
