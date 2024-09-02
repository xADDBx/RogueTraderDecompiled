using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics;

public static class PositionBasedDynamicsConstantBuffer
{
	public static int _VelocitySleepThreshold = Shader.PropertyToID("_VelocitySleepThreshold");

	public static int _Decay = Shader.PropertyToID("_Decay");

	public static int _World2Local = Shader.PropertyToID("_World2Local");

	public static int _Local2World = Shader.PropertyToID("_Local2World");

	public static int _Color = Shader.PropertyToID("_Color");

	public static int _ColorPair = Shader.PropertyToID("_ColorPair");

	public static int _Size = Shader.PropertyToID("_Size");

	public static int _Offset = Shader.PropertyToID("_Offset");

	public static int _Count = Shader.PropertyToID("_Count");

	public static int _ParticlesOffset = Shader.PropertyToID("_ParticlesOffset");

	public static int _VerticesOffset = Shader.PropertyToID("_VerticesOffset");

	public static int _PBDNormals = Shader.PropertyToID("_PBDNormals");

	public static int _PBDTangents = Shader.PropertyToID("_PBDTangents");

	public static int _PbdBindposes = Shader.PropertyToID("_PbdBindposes");

	public static int _PbdSkinnedBodyBoneIndicesMap = Shader.PropertyToID("_PbdSkinnedBodyBoneIndicesMap");

	public static int _PbdEnabledGlobal = Shader.PropertyToID("_PbdEnabledGlobal");

	public static int _Matrices = Shader.PropertyToID("_Matrices");

	public static int _Parameters0 = Shader.PropertyToID("_Parameters0");

	public static int _MaterialParameters = Shader.PropertyToID("_MaterialParameters");

	public static int _PackedEnumValues = Shader.PropertyToID("_PackedEnumValues");

	public static int _LocalToWorldVolumeMatrices = Shader.PropertyToID("_LocalToWorldVolumeMatrices");

	public static int _LocalToWorldEmitterMatrices = Shader.PropertyToID("_LocalToWorldEmitterMatrices");

	public static int _VolumeParameters0 = Shader.PropertyToID("_VolumeParameters0");

	public static int _EmitterDirection = Shader.PropertyToID("_EmitterDirection");

	public static int _VolumeParameters1 = Shader.PropertyToID("_VolumeParameters1");

	public static int _ForceVolumesCount = Shader.PropertyToID("_ForceVolumesCount");

	public static int _GlobalCollidersCount = Shader.PropertyToID("_GlobalCollidersCount");

	public static int _PbdConstraintsGroupsBuffer = Shader.PropertyToID("_PbdConstraintsGroupsBuffer");

	public static int _PbdBodyDescriptorBuffer = Shader.PropertyToID("_PbdBodyDescriptorBuffer");

	public static int _BodyGroupOffset = Shader.PropertyToID("_BodyGroupOffset");

	public static int _DeltaTime = Shader.PropertyToID("_DeltaTime");

	public static int _SimulationIterations = Shader.PropertyToID("_SimulationIterations");

	public static int _ConstraintIterations = Shader.PropertyToID("_ConstraintIterations");

	public static int _GrassParticlesCount = Shader.PropertyToID("_GrassParticlesCount");

	public static int _WorldMatrices = Shader.PropertyToID("_WorldMatrices");

	public static int _InvWorldMatrices = Shader.PropertyToID("_InvWorldMatrices");

	public static int _Counters0 = Shader.PropertyToID("_Counters0");

	public static int _Counters1 = Shader.PropertyToID("_Counters1");

	public static int _GridResolution = Shader.PropertyToID("_GridResolution");

	public static int _GridAabbBuffer = Shader.PropertyToID("_GridAabbBuffer");

	public static int _GridBuffer = Shader.PropertyToID("_GridBuffer");

	public static int _GlobalWindEnabled = Shader.PropertyToID("_GlobalWindEnabled");

	public static int _BodiesAabbOffset = Shader.PropertyToID("_BodiesAabbOffset");

	public static int _GrassBodyCount = Shader.PropertyToID("_GrassBodyCount");

	public static int _ForceVolumesAabbOffset = Shader.PropertyToID("_ForceVolumesAabbOffset");

	public static int _DebugAabbOffset = Shader.PropertyToID("_DebugAabbOffset");

	public static int _CameraCullingEnabled = Shader.PropertyToID("_CameraCullingEnabled");

	public static int _BroadphaseGridResolution = Shader.PropertyToID("_BroadphaseGridResolution");

	public static int _BroadphaseType = Shader.PropertyToID("_BroadphaseType");

	public static int _ViewProj = Shader.PropertyToID("_ViewProj");

	public static int _CamerasCount = Shader.PropertyToID("_CamerasCount");
}
