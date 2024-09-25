using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[Serializable]
public struct OcclusionGeometry
{
	[Serializable]
	public struct PackedBounds
	{
		public half3 a;

		public half3 b;

		public half3 c;

		public half3 e;

		public half4 q;

		public int ib;

		public int ie;

		public PackedBounds(ABox aBox, OBox oBox)
		{
			a = (half3)aBox.min;
			b = (half3)aBox.max;
			c = (half3)oBox.center;
			e = (half3)oBox.extents;
			q = (half4)quaternion.LookRotation(oBox.zAxis, oBox.yAxis).value;
			ib = 0;
			ie = 0;
		}

		public void Unpack(ref ABox aBox, ref OBox oBox)
		{
			float3 min = a;
			float3 max = b;
			aBox = new ABox(in min, in max);
			quaternion quaternion = new quaternion(q);
			oBox.center = c;
			oBox.extents = e;
			oBox.xAxis = math.rotate(quaternion, new float3(1f, 0f, 0f));
			oBox.yAxis = math.rotate(quaternion, new float3(0f, 1f, 0f));
			oBox.zAxis = math.rotate(quaternion, new float3(0f, 0f, 1f));
		}

		public void Unpack(ref ABox aBox)
		{
			float3 min = a;
			float3 max = b;
			aBox = new ABox(in min, in max);
		}

		public void Unpack(ref OBox oBox)
		{
			quaternion quaternion = new quaternion(q);
			oBox.center = c;
			oBox.extents = e;
			oBox.xAxis = math.rotate(quaternion, new float3(1f, 0f, 0f));
			oBox.yAxis = math.rotate(quaternion, new float3(0f, 1f, 0f));
			oBox.zAxis = math.rotate(quaternion, new float3(0f, 0f, 1f));
		}
	}

	public PackedBounds[] bounds;

	public Renderer[] renderers;

	public ushort[] indices;
}
