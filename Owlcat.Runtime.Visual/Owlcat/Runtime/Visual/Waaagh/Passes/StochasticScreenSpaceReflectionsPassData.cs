using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class StochasticScreenSpaceReflectionsPassData : PassDataBase
{
	internal ComputeShader SssrCS;

	internal ComputeShaderKernelDescriptor RaytraceKernel;

	internal ComputeShaderKernelDescriptor ReprojectionKernel;

	internal ComputeShaderKernelDescriptor AccumulateKernel;

	internal ComputeShaderKernelDescriptor BilateralBlurKernel;

	internal LocalKeyword SsrApproxKeyword;

	internal Material BlitMaterial;

	internal Material SsrBlurMaterial;

	internal int MinDepthLevel;

	internal int MaxRaySteps;

	internal float ThicknessScale;

	internal float ThicknessBias;

	internal float MaxRoughness;

	internal Vector4 SsrScreenSize;

	internal float BRDFBias;

	internal int FrameCount;

	internal float EdgeFadeRcpLength;

	internal float RoughnessFadeEndTimesRcpLength;

	internal float RoughnessFadeRcpLength;

	internal float AccumulationAmount;

	internal float SpeedRejectionScalerFactor;

	internal float SpeedRejection;

	internal bool BlurEnabled;

	internal float FresnelPower;

	internal bool UseReprojectedHistory;

	internal Vector4 RoughnessRemap;

	internal Vector4 SmoothnessRemap;

	internal bool IsStochastic;

	internal int SsrBlurPass;

	internal TextureHandle SsrRT;

	internal TextureHandle SsrHitPointRT;

	internal TextureHandle CameraDepthCopyRT;

	internal TextureHandle CameraDepthPyramidRT;

	internal TextureHandle CameraHistoryColorRT;

	internal TextureHandle CameraNormalsRT;

	internal TextureHandle CameraTranslucencyRT;

	internal TextureHandle CameraMotionVectorsRT;

	internal TextureHandle SsrRTPrev;

	internal TextureHandle SsrPyramidMips;

	internal Texture2D RankingTileXSPP;

	internal Texture2D OwenScrambledTexture;

	internal Texture2D ScramblingTileXSPP;

	internal int SsrCompositeSsrPass;

	internal bool TemporalAccumulation;
}
