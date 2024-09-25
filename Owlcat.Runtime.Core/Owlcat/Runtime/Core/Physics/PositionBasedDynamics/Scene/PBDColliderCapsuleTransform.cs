using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.ProfilingCounters;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDColliderCapsuleTransform : PBDPositionalColliderBase
{
	[Range(0f, 1f)]
	public float Radius0 = 0.5f;

	[Range(0f, 1f)]
	public float Radius1 = 0.5f;

	public Transform Center0;

	public Transform Center1;

	public override ColliderType GetColliderType()
	{
		return ColliderType.Capsule;
	}

	protected override void DoUpdateOverride()
	{
		using (Counters.PBD?.Measure())
		{
			base.DoUpdateOverride();
			float3 @float = ((Center0 == null) ? Vector3.one : Center0.lossyScale);
			float3 float2 = ((Center1 == null) ? Vector3.one : Center1.lossyScale);
			float num = math.max(@float.x, math.max(@float.y, @float.z));
			float num2 = math.max(float2.x, math.max(float2.y, float2.z));
			float4 c = new float4((Center0 == null) ? base.transform.position : Center0.position, Radius0 * num);
			float4 c2 = new float4((Center1 == null) ? base.transform.position : Center1.position, Radius1 * num2);
			m_ColliderRef.World = new float4x4(c, c2, 0, 0);
			m_ColliderRef.Parameters0 = new float4(-1f, 0f, 0f, 0f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			DoUpdateOverride();
		}
		float4x4 world = m_ColliderRef.World;
		float4 c = world.c0;
		float4 c2 = world.c1;
		Color color = Gizmos.color;
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(c.xyz, c.w);
		Gizmos.DrawWireSphere(c2.xyz, c2.w);
		float3 x = math.normalize(c.xyz - c2.xyz);
		float3 y = new float3(0f, 1f, 0f);
		if (math.dot(x, y) > 0.999f)
		{
			y = new float3(1f, 0f, 0f);
		}
		float3 @float = math.normalize(math.cross(x, y));
		float3 float2 = math.normalize(math.cross(x, @float));
		Gizmos.DrawLine(c.xyz + @float * c.w, c2.xyz + @float * c2.w);
		Gizmos.DrawLine(c.xyz - @float * c.w, c2.xyz - @float * c2.w);
		Gizmos.DrawLine(c.xyz + float2 * c.w, c2.xyz + float2 * c2.w);
		Gizmos.DrawLine(c.xyz - float2 * c.w, c2.xyz - float2 * c2.w);
		Gizmos.color = color;
	}
}
