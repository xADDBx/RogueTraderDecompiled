using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;
using Unity.Jobs;
using UnityEngine;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__3331759749
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobParallelForExtensions.EarlyJobInit<SimulationJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateBodyAabbJob>();
			IJobExtensions.EarlyJobInit<UpdateCollidersGridJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateCollidersJob>();
			IJobExtensions.EarlyJobInit<UpdateForceVolumesGridJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateForceVolumesJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateMeshAfterSimulationJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateMeshParticlesJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateSkinnedParticlesJob>();
			IJobExtensions.EarlyJobInit<CalculateSceneAabbJob>();
			IJobParallelForExtensions.EarlyJobInit<ClearSpatialHashmapJob>();
			IJobParallelForExtensions.EarlyJobInit<GridCalculateHashJob>();
			IJobExtensions.EarlyJobInit<GridFindCellStartJob>();
			IJobParallelForExtensions.EarlyJobInit<GridFindOverlappingPairsJob>();
			IJobExtensions.EarlyJobInit<SpatialHashmapBuildJob>();
			IJobParallelForExtensions.EarlyJobInit<SpatialHashmapFindPairsJob>();
			IJobExtensions.EarlyJobInit<SpatialHasmapLoadFactorJob>();
			IJobExtensions.EarlyJobInit<QuickSortJob<KeyValuePairComparable<uint, uint>>>();
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
