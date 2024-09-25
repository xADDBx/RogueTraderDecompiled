using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

[Serializable]
public class BroadphaseSettings
{
	[SerializeField]
	private BroadphaseType m_Type;

	[SerializeField]
	private SimpleGridSettings m_SimpleGridSettings;

	[SerializeField]
	private MultilevelGridSettings m_MultilevelGridSettings;

	[SerializeField]
	private OptimizedSpatialHashingSettings m_OptimizedSpatialHashingSettings;

	public BroadphaseType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public SimpleGridSettings SimpleGridSettings => m_SimpleGridSettings;

	public MultilevelGridSettings MultilevelGridSettings => m_MultilevelGridSettings;

	public OptimizedSpatialHashingSettings OptimizedSpatialHashingSettings => m_OptimizedSpatialHashingSettings;
}
