using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDColliderBox : PBDPositionalColliderBase
{
	public float3 Size = 1;

	public override ColliderType GetColliderType()
	{
		return ColliderType.Box;
	}

	protected override void DoUpdateOverride()
	{
		base.DoUpdateOverride();
		m_ColliderRef.World = base.transform.localToWorldMatrix;
		m_ColliderRef.Parameters0 = new float4(Size, 0f);
	}

	private void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			Color color = Gizmos.color;
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.color = Color.green;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Size);
			Gizmos.color = color;
			Gizmos.matrix = matrix;
		}
	}
}
