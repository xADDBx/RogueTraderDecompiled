using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.ProfilingCounters;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDColliderSphere : PBDPositionalColliderBase
{
	public float3 Position;

	public float Radius = 0.5f;

	public override ColliderType GetColliderType()
	{
		return ColliderType.Sphere;
	}

	protected override void DoUpdateOverride()
	{
		using (Counters.PBD?.Measure())
		{
			base.DoUpdateOverride();
			m_ColliderRef.World = base.transform.localToWorldMatrix;
			m_ColliderRef.Parameters0 = new float4(Position, Radius);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			Vector3 center = base.transform.TransformPoint(Position);
			float3 @float = base.transform.lossyScale;
			float num = math.max(@float.x, math.max(@float.y, @float.z));
			float radius = Radius * num;
			Color color = Gizmos.color;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(center, radius);
			Gizmos.color = color;
		}
	}
}
