using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.ProfilingCounters;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDColliderPlane : PBDColliderBase
{
	public override ColliderType GetColliderType()
	{
		return ColliderType.Plane;
	}

	protected override void DoUpdateOverride()
	{
		using (Counters.PBD?.Measure())
		{
			base.DoUpdateOverride();
			Matrix4x4 matrix4x = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
			m_ColliderRef.World = matrix4x;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			Color color = Gizmos.color;
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.color = Color.green;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(2f, 0f, 2f));
			Gizmos.color = color;
			Gizmos.matrix = matrix;
		}
	}
}
