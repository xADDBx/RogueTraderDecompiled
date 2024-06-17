using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh;

public static class ShaderPropertyId
{
	public static readonly int _GlossyEnvironmentColor = Shader.PropertyToID("_GlossyEnvironmentColor");

	public static readonly int _SubtractiveShadowColor = Shader.PropertyToID("_SubtractiveShadowColor");

	public static readonly int _GlossyEnvironmentCubeMap = Shader.PropertyToID("_GlossyEnvironmentCubeMap");

	public static readonly int _GlossyEnvironmentCubeMapHDR = Shader.PropertyToID("_GlossyEnvironmentCubeMap_HDR");

	public static readonly int unity_AmbientSky = Shader.PropertyToID("unity_AmbientSky");

	public static readonly int unity_AmbientEquator = Shader.PropertyToID("unity_AmbientEquator");

	public static readonly int unity_AmbientGround = Shader.PropertyToID("unity_AmbientGround");

	public static readonly int _ProjectionParams = Shader.PropertyToID("_ProjectionParams");

	public static readonly int _WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");

	public static readonly int _ScreenParams = Shader.PropertyToID("_ScreenParams");

	public static readonly int _ScaledScreenParams = Shader.PropertyToID("_ScaledScreenParams");

	public static readonly int _ZBufferParams = Shader.PropertyToID("_ZBufferParams");

	public static readonly int unity_OrthoParams = Shader.PropertyToID("unity_OrthoParams");

	public static readonly int _ScreenSize = Shader.PropertyToID("_ScreenSize");

	public static readonly int _GlobalMipBias = Shader.PropertyToID("_GlobalMipBias");

	public static readonly int _Time = Shader.PropertyToID("_Time");

	public static readonly int _SinTime = Shader.PropertyToID("_SinTime");

	public static readonly int _CosTime = Shader.PropertyToID("_CosTime");

	public static readonly int _UnscaledTime = Shader.PropertyToID("_UnscaledTime");

	public static readonly int unity_DeltaTime = Shader.PropertyToID("unity_DeltaTime");

	public static readonly int _TimeParameters = Shader.PropertyToID("_TimeParameters");

	public static readonly int _UnscaledTimeParameters = Shader.PropertyToID("_UnscaledTimeParameters");

	public static readonly int unity_MatrixInvV = Shader.PropertyToID("unity_MatrixInvV");

	public static readonly int unity_MatrixInvP = Shader.PropertyToID("unity_MatrixInvP");

	public static readonly int unity_MatrixInvVP = Shader.PropertyToID("unity_MatrixInvVP");

	public static readonly int _InvProjMatrix = Shader.PropertyToID("_InvProjMatrix");

	public static readonly int _InvCameraViewProj = Shader.PropertyToID("_InvCameraViewProj");

	public static readonly int _CamBasisUp = Shader.PropertyToID("_CamBasisUp");

	public static readonly int _CamBasisSide = Shader.PropertyToID("_CamBasisSide");

	public static readonly int _CamBasisFront = Shader.PropertyToID("_CamBasisFront");

	public static readonly int unity_CameraProjection = Shader.PropertyToID("unity_CameraProjection");

	public static readonly int unity_CameraInvProjection = Shader.PropertyToID("unity_CameraInvProjection");

	public static readonly int unity_WorldToCamera = Shader.PropertyToID("unity_WorldToCamera");

	public static readonly int unity_CameraToWorld = Shader.PropertyToID("unity_CameraToWorld");

	public static readonly int unity_CameraWorldClipPlanes = Shader.PropertyToID("unity_CameraWorldClipPlanes");

	public static readonly int unity_BillboardNormal = Shader.PropertyToID("unity_BillboardNormal");

	public static readonly int unity_BillboardTangent = Shader.PropertyToID("unity_BillboardTangent");

	public static readonly int unity_BillboardCameraParams = Shader.PropertyToID("unity_BillboardCameraParams");

	public static readonly int _PrevViewProjMatrix = Shader.PropertyToID("_PrevViewProjMatrix");

	public static readonly int _NonJitteredViewProjMatrix = Shader.PropertyToID("_NonJitteredViewProjMatrix");

	public static readonly int _NonJitteredProjMatrix = Shader.PropertyToID("_NonJitteredProjMatrix");

	public static readonly int _CameraColorRT = Shader.PropertyToID("_CameraColorRT");

	public static readonly int _CameraDepthRT = Shader.PropertyToID("_CameraDepthRT");

	public static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");

	public static readonly int _CameraDepthAttachment = Shader.PropertyToID("_CameraDepthAttachment");

	public static readonly int _CameraDepthPyramidRT = Shader.PropertyToID("_CameraDepthPyramidRT");

	public static readonly int _CameraAlbedoRT = Shader.PropertyToID("_CameraAlbedoRT");

	public static readonly int _CameraSpecularRT = Shader.PropertyToID("_CameraSpecularRT");

	public static readonly int _CameraNormalsRT = Shader.PropertyToID("_CameraNormalsRT");

	public static readonly int _CameraNormalsTexture = Shader.PropertyToID("_CameraNormalsTexture");

	public static readonly int _CameraEmissionRT = Shader.PropertyToID("_CameraEmissionRT");

	public static readonly int _CameraBakedGIRT = Shader.PropertyToID("_CameraBakedGIRT");

	public static readonly int _CameraShadowmaskRT = Shader.PropertyToID("_CameraShadowmaskRT");

	public static readonly int _CameraTranslucencyRT = Shader.PropertyToID("_CameraTranslucencyRT");

	public static readonly int _CameraDeferredReflectionsRT = Shader.PropertyToID("_CameraDeferredReflectionsRT");

	public static readonly int _CameraColorPyramidRT = Shader.PropertyToID("_CameraColorPyramidRT");

	public static readonly int _DistortionVectorsRT = Shader.PropertyToID("_DistortionVectorsRT");

	public static readonly int _DecalsNormalsRT = Shader.PropertyToID("_DecalsNormalsRT");

	public static readonly int _DecalsMasksRT = Shader.PropertyToID("_DecalsMasksRT");

	public static readonly int _SsrRT = Shader.PropertyToID("_SsrRT");

	public static readonly int _CustomPostProcessInput = Shader.PropertyToID("_CustomPostProcessInput");

	public static readonly int LightDataConstantBuffer = Shader.PropertyToID("LightDataConstantBuffer");

	public static readonly int LightVolumeDataCB = Shader.PropertyToID("LightVolumeDataCB");

	public static readonly int ZBinsCB = Shader.PropertyToID("ZBinsCB");

	public static readonly int _LightDataParams = Shader.PropertyToID("_LightDataParams");

	public static readonly int _ClusteringParams = Shader.PropertyToID("_ClusteringParams");

	public static readonly int _LightTilesBuffer = Shader.PropertyToID("_LightTilesBuffer");

	public static readonly int _LightTilesBufferUAV = Shader.PropertyToID("_LightTilesBufferUAV");

	public static readonly int _LightTilesBufferUAVSize = Shader.PropertyToID("_LightTilesBufferUAVSize");

	public static readonly int _ScreenProjMatrix = Shader.PropertyToID("_ScreenProjMatrix");

	public static readonly int _TilesMinMaxZTexture = Shader.PropertyToID("_TilesMinMaxZTexture");

	public static readonly int _ShadowmapRT = Shader.PropertyToID("_ShadowmapRT");

	public static readonly int _FaceVectors = Shader.PropertyToID("_FaceVectors");

	public static readonly int _ShadowMatricesBuffer = Shader.PropertyToID("_ShadowMatricesBuffer");

	public static readonly int _ShadowDataBuffer = Shader.PropertyToID("_ShadowDataBuffer");

	public static readonly int _ShadowFadeDistanceScaleAndBias = Shader.PropertyToID("_ShadowFadeDistanceScaleAndBias");

	public static readonly int _ShadowEntryIndex = Shader.PropertyToID("_ShadowEntryIndex");

	public static readonly int _OffsetFactor = Shader.PropertyToID("_OffsetFactor");

	public static readonly int _OffsetUnits = Shader.PropertyToID("_OffsetUnits");

	public static readonly int _Clips = Shader.PropertyToID("_Clips");

	public static readonly int _FaceId = Shader.PropertyToID("_FaceId");

	public static readonly int _ZClip = Shader.PropertyToID("_ZClip");

	public static readonly int _ShadowReceiverNormalBias = Shader.PropertyToID("_ShadowReceiverNormalBias");

	public static readonly int _ShadowAtlasSize = Shader.PropertyToID("_ShadowAtlasSize");

	public static readonly int _GlobalShadowsEnabled = Shader.PropertyToID("_GlobalShadowsEnabled");

	public static readonly int _DirectionalCascadesCount = Shader.PropertyToID("_DirectionalCascadesCount");

	public static readonly int ShadowConstantBuffer = Shader.PropertyToID("ShadowConstantBuffer");

	public static readonly int ShadowMatricesConstantBuffer = Shader.PropertyToID("ShadowMatricesConstantBuffer");

	public static readonly int ShadowCopyCacheConstantBuffer = Shader.PropertyToID("ShadowCopyCacheConstantBuffer");

	public static readonly int _BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");

	public static readonly int _BlitTexture = Shader.PropertyToID("_BlitTexture");

	public static readonly int _BlitTex = Shader.PropertyToID("_BlitTex");

	public static readonly int _BlitMipLevel = Shader.PropertyToID("_BlitMipLevel");

	public static readonly int _GBuffer0 = Shader.PropertyToID("_GBuffer0");

	public static readonly int _GBuffer1 = Shader.PropertyToID("_GBuffer1");

	public static readonly int _GBuffer2 = Shader.PropertyToID("_GBuffer2");

	public static readonly int _GBuffer3 = Shader.PropertyToID("_GBuffer3");

	public static readonly int _GBuffer4 = Shader.PropertyToID("_GBuffer4");

	public static readonly int _GBuffer5 = Shader.PropertyToID("_GBuffer5");

	public static readonly int _SpecCube0 = Shader.PropertyToID("_SpecCube0");

	public static readonly int _SpecCube0_HDR = Shader.PropertyToID("_SpecCube0_HDR");

	public static readonly int _UseBoxProjection = Shader.PropertyToID("_UseBoxProjection");

	public static readonly int _SpecCube0_ProbePosition = Shader.PropertyToID("_SpecCube0_ProbePosition");

	public static readonly int _SpecCube0_BoxMin = Shader.PropertyToID("_SpecCube0_BoxMin");

	public static readonly int _SpecCube0_BoxMax = Shader.PropertyToID("_SpecCube0_BoxMax");

	public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

	public static readonly int _StencilComp = Shader.PropertyToID("_StencilComp");

	public static int _Source = Shader.PropertyToID("_Source");

	public static int _SrcScaleBias = Shader.PropertyToID("_SrcScaleBias");

	public static int _SrcUvLimits = Shader.PropertyToID("_SrcUvLimits");

	public static int _SourceMip = Shader.PropertyToID("_SourceMip");

	public static int _ColorPyramidLodCount = Shader.PropertyToID("_ColorPyramidLodCount");

	public static int _DepthPyramidSamplingRatio = Shader.PropertyToID("_DepthPyramidSamplingRatio");

	public static int _HexRatio = Shader.PropertyToID("_HexRatio");

	public static int _OccludedObjectClipNoiseTiling = Shader.PropertyToID("_OccludedObjectClipNoiseTiling");

	public static int _OccludedObjectClipTreshold = Shader.PropertyToID("_OccludedObjectClipTreshold");

	public static int _OccludedObjectAlphaScale = Shader.PropertyToID("_OccludedObjectAlphaScale");

	public static int _OccludedObjectClipNearCameraDistance = Shader.PropertyToID("_OccludedObjectClipNearCameraDistance");

	public static int _OccludedObjectHighlightingFeatureEnabled = Shader.PropertyToID("_OccludedObjectHighlightingFeatureEnabled");

	public static int _OccluderObjectOpacity = Shader.PropertyToID("_OccluderObjectOpacity");

	public static int _VolumeScatter = Shader.PropertyToID("_VolumeScatter");

	public static int _VolumetricLightingEnabled = Shader.PropertyToID("_VolumetricLightingEnabled");

	public static int _FogTilesBuffer = Shader.PropertyToID("_FogTilesBuffer");

	public static int _FogTilesBufferUAV = Shader.PropertyToID("_FogTilesBufferUAV");

	public static int _FogTilesBufferUAVSize = Shader.PropertyToID("_FogTilesBufferUAVSize");

	public static int _VisibleVolumeBoundsBuffer = Shader.PropertyToID("_VisibleVolumeBoundsBuffer");

	public static int _VisibleVolumeDataBuffer = Shader.PropertyToID("_VisibleVolumeDataBuffer");

	public static int _LocalFogVolumesCount = Shader.PropertyToID("_LocalFogVolumesCount");

	public static int _LocalVolumetricFogClusteringParams = Shader.PropertyToID("_LocalVolumetricFogClusteringParams");

	public static int _LocalFogZBinsBuffer = Shader.PropertyToID("_LocalFogZBinsBuffer");

	public static int ShaderVariablesBilateralUpsample = Shader.PropertyToID("ShaderVariablesBilateralUpsample");

	public static int _DepthTexture = Shader.PropertyToID("_DepthTexture");

	public static int _LowResolutionTexture = Shader.PropertyToID("_LowResolutionTexture");

	public static int _OutputUpscaledTexture = Shader.PropertyToID("_OutputUpscaledTexture");

	public static int _DebugMaterialMode = Shader.PropertyToID("_DebugMaterialMode");

	public static int _DebugLightingMode = Shader.PropertyToID("_DebugLightingMode");

	public static int _DebugOverdrawMode = Shader.PropertyToID("_DebugOverdrawMode");

	public static int _DebugColor = Shader.PropertyToID("_DebugColor");

	public static int _DebugMipMap = Shader.PropertyToID("_DebugMipMap");

	public static int _MipMapDebugMap = Shader.PropertyToID("_MipMapDebugMap");
}
