using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

[Serializable]
public class MultilevelGridSettings
{
	[SerializeField]
	private MultilevelGridDimension m_Dimemsion = MultilevelGridDimension.Grid3D;

	[SerializeField]
	[Min(0.1f)]
	private float m_CellSize = 1f;

	public MultilevelGridDimension Dimension
	{
		get
		{
			return m_Dimemsion;
		}
		set
		{
			m_Dimemsion = value;
		}
	}

	public float CellSize
	{
		get
		{
			return m_CellSize;
		}
		set
		{
			m_CellSize = math.max(0.1f, value);
		}
	}
}
