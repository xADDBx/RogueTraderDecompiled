using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

[Serializable]
public class SimpleGridSettings
{
	[Range(2f, 128f)]
	private int m_GridResolution = 100;

	public int GridResolution
	{
		get
		{
			return m_GridResolution;
		}
		set
		{
			m_GridResolution = value;
		}
	}
}
