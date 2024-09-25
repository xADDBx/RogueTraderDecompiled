using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.ProfilingCounters;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDColliderCapsule : PBDPositionalColliderBase
{
	[Range(0f, 1f)]
	public float Radius0 = 0.5f;

	[Range(0f, 1f)]
	public float Radius1 = 0.5f;

	public float Height = 2f;

	public CapsuleColliderDirection Direction;

	public override ColliderType GetColliderType()
	{
		return ColliderType.Capsule;
	}

	protected override void DoUpdateOverride()
	{
		using (Counters.PBD?.Measure())
		{
			base.DoUpdateOverride();
			m_ColliderRef.World = base.transform.localToWorldMatrix;
			m_ColliderRef.Parameters0 = new float4((float)Direction, Radius0, Radius1, Height);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			float3 @float = base.transform.lossyScale;
			float num = 1f;
			float num2 = 1f;
			float3 float2 = base.transform.up;
			float3 float3 = base.transform.right;
			float3 float4 = base.transform.forward;
			switch (Direction)
			{
			case CapsuleColliderDirection.AxisY:
				num = @float.y;
				num2 = math.max(@float.x, @float.z);
				float2 = base.transform.up;
				float3 = base.transform.right;
				float4 = base.transform.forward;
				break;
			case CapsuleColliderDirection.AxisZ:
				num = @float.z;
				num2 = math.max(@float.x, @float.y);
				float2 = base.transform.forward;
				float3 = base.transform.right;
				float4 = base.transform.up;
				break;
			case CapsuleColliderDirection.AxisX:
				num = @float.x;
				num2 = math.max(@float.y, @float.z);
				float2 = base.transform.right;
				float3 = base.transform.up;
				float4 = base.transform.forward;
				break;
			}
			Color color = Gizmos.color;
			Gizmos.color = Color.green;
			float num3 = Height * 0.5f;
			float3 float5 = (float3)base.transform.position - float2 * num3 * num + float2 * Radius0 * num2;
			Gizmos.DrawWireSphere(float5, Radius0 * num2);
			float3 float6 = (float3)base.transform.position + float2 * num3 * num - float2 * Radius1 * num2;
			Gizmos.DrawWireSphere(float6, Radius1 * num2);
			Gizmos.DrawLine(float5 + float3 * Radius0 * num2, float6 + float3 * Radius1 * num2);
			Gizmos.DrawLine(float5 - float3 * Radius0 * num2, float6 - float3 * Radius1 * num2);
			Gizmos.DrawLine(float5 + float4 * Radius0 * num2, float6 + float4 * Radius1 * num2);
			Gizmos.DrawLine(float5 - float4 * Radius0 * num2, float6 - float4 * Radius1 * num2);
			Gizmos.color = color;
		}
	}
}
