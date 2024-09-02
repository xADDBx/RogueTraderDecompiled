using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using Owlcat.Runtime.Core.ProfilingCounters;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics;

public static class PBD
{
	private static class ShaderConstants
	{
		public static int _PbdBindposes = Shader.PropertyToID("_PbdBindposes");

		public static int _PbdParticlesBasePositionBuffer = Shader.PropertyToID("_PbdParticlesBasePositionBuffer");

		public static int _PbdParticlesPositionBuffer = Shader.PropertyToID("_PbdParticlesPositionBuffer");

		public static int _PBDNormals = Shader.PropertyToID("_PBDNormals");

		public static int _PBDTangents = Shader.PropertyToID("_PBDTangents");

		public static int _PbdSkinnedBodyBoneIndicesMap = Shader.PropertyToID("_PbdSkinnedBodyBoneIndicesMap");

		public static int _PbdBodyWorldToLocalMatrices = Shader.PropertyToID("_PbdBodyWorldToLocalMatrices");
	}

	private const float kUpdatePeriod30Fps = 1f / 30f;

	private const float kUpdatePeriod45Fps = 1f / 45f;

	private const float kUpdatePeriod60Fps = 1f / 60f;

	public const int kInnerLoopBatchCount = 1;

	public const float kSleepThreshold = 1f;

	public const float kFloatEpsilon = 1E-06f;

	public const int kMaxCollisionContactsCount = 2048;

	public const int kMaxArraySizePerUpdateOnGpu = 256;

	public const int kMaxTriangleCountPerMeshBody = 1280;

	public const float kMemoryIncreaseScale = 1.5f;

	private static TimeService s_TimeService;

	private static Simulation s_Simulation;

	private static bool s_PrevEnabled;

	private static PositionBasedDynamicsConfig s_Config;

	private static PBDSceneController s_SceneController;

	private static int s_LastFrameDebug;

	internal static Action OnBeforeSimulationTick;

	internal static Action OnAfterUpdate;

	internal static Action<HashSet<Body>> OnBodyDataUpdated;

	public static Action OnSceneInitializationFinished;

	public static bool IsGpu
	{
		get
		{
			if (s_Simulation != null)
			{
				return s_Simulation.IsGPU;
			}
			return false;
		}
	}

	public static bool IsEmpty
	{
		get
		{
			if (s_Simulation != null)
			{
				return s_Simulation.IsEmpty;
			}
			return true;
		}
	}

	public static bool IsSceneInitialization => s_Simulation?.IsSceneInitialization ?? false;

	public static bool WillSimulateOnCurrentFrame
	{
		get
		{
			TimeService timeService = s_TimeService;
			if (timeService != null && timeService.WillSimulateOnCurrentFrame)
			{
				return UpdatePeriod > 0f;
			}
			return false;
		}
	}

	public static UpdateMode UpdateMode => s_Config?.UpdateMode ?? UpdateMode.GameTime;

	public static bool UseExperimentalFeatures => s_Config?.UseExperimentalFeatures ?? false;

	public static float UnscaledUpdatePeriod => s_Config.UpdateFrequency switch
	{
		UpdateFrequency.Fps30 => 1f / 30f, 
		UpdateFrequency.Fps60 => 1f / 60f, 
		UpdateFrequency.Fps45 => 1f / 45f, 
		_ => 1f / 30f, 
	};

	public static float UpdatePeriod
	{
		get
		{
			float num = UnscaledUpdatePeriod;
			if (UpdateMode == UpdateMode.GameTime)
			{
				num = ((!(Time.timeScale > 0f)) ? 0f : (num * Time.timeScale));
			}
			return num;
		}
	}

	public static BroadphaseSettings BroadphaseSettings => s_Config.BroadphaseSettings;

	public static PositionBaseDynamicsDebugSettings DebugSettings => s_Config.DebugSettings;

	static PBD()
	{
		if (Application.isPlaying)
		{
			s_Config = PositionBasedDynamicsConfig.Instance;
			s_Simulation = new Simulation(s_Config.GPU, s_Config.SimulationIterations, s_Config.ConstraintIterations, s_Config.Decay);
			s_TimeService = new TimeService();
			s_SceneController = new PBDSceneController();
			s_PrevEnabled = s_Config.Enabled;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnPlay()
	{
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		int num = Array.FindIndex(currentPlayerLoop.subSystemList, (PlayerLoopSystem s) => s.type == typeof(PreUpdate));
		List<PlayerLoopSystem> list = currentPlayerLoop.subSystemList[num].subSystemList.ToList();
		list.Add(new PlayerLoopSystem
		{
			type = typeof(PBD),
			updateDelegate = UpdatePBD
		});
		currentPlayerLoop.subSystemList[num].subSystemList = list.ToArray();
		PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		SceneManager.sceneUnloaded += OnSceneUnloaded;
		Application.quitting += OnApplicationQuit;
	}

	private static void OnSceneUnloaded(UnityEngine.SceneManagement.Scene arg0)
	{
		if (s_Simulation != null)
		{
			s_Simulation.MemoryReset();
		}
	}

	private static void OnApplicationQuit()
	{
		if (s_Simulation != null)
		{
			s_Simulation.Dispose();
			s_Simulation = null;
		}
	}

	public static void MemoryReset()
	{
		if (s_Simulation != null)
		{
			s_Simulation.MemoryReset();
		}
	}

	internal static void RegisterSceneBody(PBDBodyBase sceneBody)
	{
		if (s_SceneController != null)
		{
			s_SceneController.RegisterBody(sceneBody);
		}
	}

	internal static void UnregisterSceneBody(PBDBodyBase sceneBody)
	{
		if (s_SceneController != null)
		{
			s_SceneController.UnregisterBody(sceneBody);
		}
	}

	public static void RegisterBody(Body body)
	{
		if (s_Simulation != null)
		{
			s_Simulation.RegisterBody(body);
		}
	}

	public static void UnregisterBody(Body body)
	{
		if (s_Simulation != null)
		{
			s_Simulation.UnregisterBody(body);
		}
	}

	internal static int GetBodyDescriptorIndex(Body body)
	{
		if (s_Simulation == null)
		{
			return -1;
		}
		return s_Simulation.GetBodyDescriptorIndex(body);
	}

	internal static void RefreshBodyParameters(Body body)
	{
		if (s_Simulation != null)
		{
			s_Simulation.RefreshBodyParameters(body);
		}
	}

	public static void RegisterForce(IForce force)
	{
		if (s_Simulation != null)
		{
			s_Simulation.RegisterForce(force);
		}
	}

	public static void UnregisterForce(IForce force)
	{
		if (s_Simulation != null)
		{
			s_Simulation.UnregisterForce(force);
		}
	}

	public static IEnumerable<IForce> GetForces()
	{
		return s_Simulation.GetForces();
	}

	internal static void RegisterCollider(ColliderRef colliderRef)
	{
		if (s_Simulation != null)
		{
			s_Simulation.RegisterCollider(colliderRef);
		}
	}

	internal static void RegisterForceVolume(ForceVolume forceVolume)
	{
		if (s_Simulation != null)
		{
			s_Simulation.RegisterForceVolume(forceVolume);
		}
	}

	internal static void UnregisterCollider(ColliderRef colliderRef)
	{
		if (s_Simulation != null)
		{
			s_Simulation.UnregisterCollider(colliderRef);
		}
	}

	internal static void UnregisterLocalColliders(Body body)
	{
		if (s_Simulation != null)
		{
			s_Simulation.UnregisterLocalColliders(body);
		}
	}

	internal static void UnregisterForceVolume(ForceVolume forceVolume)
	{
		if (s_Simulation != null)
		{
			s_Simulation.UnregisterForceVolume(forceVolume);
		}
	}

	internal static void GetParticles(Body body, out ParticleSoASlice particles)
	{
		if (s_Simulation == null)
		{
			particles = default(ParticleSoASlice);
		}
		else
		{
			s_Simulation.GetParticles(body, out particles);
		}
	}

	public static int GetParticlesOffset(Body body)
	{
		if (s_Simulation == null)
		{
			return -1;
		}
		return s_Simulation.GetParticlesOffset(body);
	}

	internal static Constraint GetConstraint(int constraintId)
	{
		if (s_Simulation == null)
		{
			return default(Constraint);
		}
		return s_Simulation.GetConstraint(constraintId);
	}

	internal static void SetConstraint(ref Constraint constraint)
	{
		if (s_Simulation != null)
		{
			s_Simulation.SetConstraint(ref constraint);
		}
	}

	internal static void UpdateDirtyBodyData(Body body)
	{
		if (s_Simulation != null)
		{
			s_Simulation.UpdateDirtyBodyData(body);
		}
	}

	public static GPUData GetGPUData()
	{
		if (s_Simulation == null)
		{
			return null;
		}
		return s_Simulation.GetGPUData();
	}

	internal static MeshBodyVerticesSoA GetMeshData()
	{
		if (s_Simulation == null)
		{
			return null;
		}
		return s_Simulation.GetMeshData();
	}

	public static IEnumerable<Body> GetBodies()
	{
		if (s_Simulation == null)
		{
			return null;
		}
		return s_Simulation.GetBodies();
	}

	public static void BeginSceneInitialization()
	{
		if (s_Simulation != null)
		{
			s_Simulation.BeginSceneInitialization();
		}
	}

	public static void EndSceneInitialization()
	{
		if (s_Simulation != null)
		{
			s_Simulation.EndSceneInitialization();
		}
	}

	private static void UpdatePBD()
	{
		if (!Application.isPlaying || s_Simulation == null || s_Simulation.IsSceneInitialization)
		{
			return;
		}
		if (s_PrevEnabled != s_Config.Enabled)
		{
			if (s_Config.Enabled)
			{
				MemoryReset();
			}
			s_PrevEnabled = s_Config.Enabled;
		}
		using (Counters.PBD?.Measure())
		{
			s_TimeService.Tick(UpdateMode);
			s_Simulation.Simulate();
			s_SceneController.Tick();
		}
	}

	public static void SetDummyComputeBuffer(CommandBuffer cmd, ComputeBuffer dummyComputeBuffer)
	{
		if (!IsGpu)
		{
			cmd.SetGlobalBuffer(ShaderConstants._PbdBindposes, dummyComputeBuffer);
			cmd.SetGlobalBuffer(ShaderConstants._PBDNormals, dummyComputeBuffer);
			cmd.SetGlobalBuffer(ShaderConstants._PbdParticlesBasePositionBuffer, dummyComputeBuffer);
			cmd.SetGlobalBuffer(ShaderConstants._PbdParticlesPositionBuffer, dummyComputeBuffer);
			cmd.SetGlobalBuffer(ShaderConstants._PBDTangents, dummyComputeBuffer);
			cmd.SetGlobalBuffer(ShaderConstants._PbdSkinnedBodyBoneIndicesMap, dummyComputeBuffer);
			cmd.SetGlobalBuffer(ShaderConstants._PbdBodyWorldToLocalMatrices, dummyComputeBuffer);
		}
	}

	public static void DrawGizmos(Body body)
	{
		if (s_Simulation != null && body != null)
		{
			s_Simulation.DrawGizmos(body);
		}
	}
}
