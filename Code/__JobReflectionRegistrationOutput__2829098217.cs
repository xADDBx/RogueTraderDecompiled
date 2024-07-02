using System;
using Kingmaker.Controllers.FogOfWar.Culling;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles.SnapController;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__2829098217
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobParallelForTransformExtensions.EarlyJobInit<ProceduralMeshSnap.GenerateVertexBufferJob>();
			IJobExtensions.EarlyJobInit<ProceduralMeshSnap.GenerateIndexBufferJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<TransformSnap.BuildBoneTransformsJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<BoneUpdateJob>();
			IJobExtensions.EarlyJobInit<BuildBezierPointsJob>();
			IJobExtensions.EarlyJobInit<BuildPathJob.Job>();
			IJobExtensions.EarlyJobInit<BuildSurfaceJob.Job>();
			IJobExtensions.EarlyJobInit<CombatHudPathProgressTracker.UpdateProgressJob>();
			IJobExtensions.EarlyJobInit<ResolveCellsJob.Job>();
			IJobParallelForExtensions.EarlyJobInit<BuildBlockerPlaneSetsJob>();
			IJobParallelForExtensions.EarlyJobInit<BuildBlockerPlanesJob>();
			IJobParallelForExtensions.EarlyJobInit<CullJob>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
