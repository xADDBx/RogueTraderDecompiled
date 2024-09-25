using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics;

[Serializable]
public class PositionBaseDynamicsDebugSettings
{
	public bool Enabled;

	public bool ShowBoundingBoxes;

	public bool ShowBroadphaseStructure;

	[SerializeField]
	private bool m_UpdateEveryFrame;

	public float ParticleSize = 0.1f;

	public Color ParticleColor = Color.red;

	public Color ConstraintColor = Color.yellow;

	public bool ShowNormals;

	public Color NormalsColor = Color.blue;

	public Color ColliderColor = Color.green;

	public Color ForceVolumeColor = Color.red;

	public Color BodyColor = Color.blue;

	public Color BodyColliderPairColor = Color.magenta;

	public Color BodyForceVolumePairColor = Color.cyan;

	public Color BroadphaseStructureColor = Color.yellow;

	public bool UpdateEveryFrame
	{
		get
		{
			if (Application.isEditor)
			{
				return m_UpdateEveryFrame;
			}
			return false;
		}
		set
		{
			m_UpdateEveryFrame = value;
		}
	}
}
