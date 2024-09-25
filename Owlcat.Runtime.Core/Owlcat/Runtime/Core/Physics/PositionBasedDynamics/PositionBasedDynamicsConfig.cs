using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics;

[CreateAssetMenu(fileName = "PositionBasedDynamicsConfig")]
public class PositionBasedDynamicsConfig : ScriptableObject
{
	internal const string kFileName = "PositionBasedDynamicsConfig";

	private static PositionBasedDynamicsConfig s_Instance;

	[SerializeField]
	private PositionBaseDynamicsDebugSettings m_DebugSettings = new PositionBaseDynamicsDebugSettings();

	public bool Enabled = true;

	public bool CameraCullingEnabled = true;

	public bool GPU;

	public bool UseExperimentalFeatures;

	public UpdateFrequency UpdateFrequency;

	public UpdateMode UpdateMode;

	[Range(1f, 10f)]
	public int SimulationIterations = 4;

	[Range(1f, 16f)]
	public int ConstraintIterations = 4;

	[Range(0f, 1f)]
	public float Decay = 0.99f;

	[SerializeField]
	private BroadphaseSettings m_BroadphaseSettings = new BroadphaseSettings();

	public PositionBaseDynamicsDebugSettings DebugSettings => m_DebugSettings;

	public BroadphaseSettings BroadphaseSettings => m_BroadphaseSettings;

	public static PositionBasedDynamicsConfig Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = Resources.Load<PositionBasedDynamicsConfig>("PositionBasedDynamicsConfig");
			}
			return s_Instance;
		}
	}
}
