using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Effects.LineRenderer;

[Serializable]
public class Line
{
	public float2 UvOffset;

	public float WidthScale = 1f;

	public float3[] Positions = new float3[0];
}
