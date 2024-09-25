using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

[Serializable]
public class OptimizedSpatialHashingSettings
{
	[SerializeField]
	[Min(0.1f)]
	private float m_CellSize = 1f;

	public float CellSize
	{
		get
		{
			return m_CellSize;
		}
		set
		{
			m_CellSize = value;
		}
	}
}
