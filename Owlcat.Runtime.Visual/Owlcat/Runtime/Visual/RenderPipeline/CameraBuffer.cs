using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class CameraBuffer
{
	public static int _InvCameraViewProj = Shader.PropertyToID("_InvCameraViewProj");

	public static int _InvProjMatrix = Shader.PropertyToID("_InvProjMatrix");

	public static int unity_MatrixInvP = Shader.PropertyToID("unity_MatrixInvP");

	public static int _CamBasisUp = Shader.PropertyToID("_CamBasisUp");

	public static int _CamBasisSide = Shader.PropertyToID("_CamBasisSide");

	public static int _CamBasisFront = Shader.PropertyToID("_CamBasisFront");

	public static int _ScreenSize = Shader.PropertyToID("_ScreenSize");

	public static int _ScreenProjMatrix = Shader.PropertyToID("_ScreenProjMatrix");

	public static int _OccludedObjectClipNoiseTiling = Shader.PropertyToID("_OccludedObjectClipNoiseTiling");

	public static int _OccludedObjectClipTreshold = Shader.PropertyToID("_OccludedObjectClipTreshold");

	public static int _OccludedObjectAlphaScale = Shader.PropertyToID("_OccludedObjectAlphaScale");

	public static int _OccludedObjectClipNearCameraDistance = Shader.PropertyToID("_OccludedObjectClipNearCameraDistance");

	public static int _OccludedObjectHighlightingFeatureEnabled = Shader.PropertyToID("_OccludedObjectHighlightingFeatureEnabled");

	public static int _ObjectSaturationAuraFeatureEnabled = Shader.PropertyToID("_ObjectSaturationAuraFeatureEnabled");

	public static int _ClusteringParams = Shader.PropertyToID("_ClusteringParams");

	public static int _LightDataParams = Shader.PropertyToID("_LightDataParams");
}
