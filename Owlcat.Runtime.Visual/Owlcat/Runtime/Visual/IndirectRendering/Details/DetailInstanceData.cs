using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.IndirectRendering.Details;

[Serializable]
public class DetailInstanceData
{
	public Vector3 Position;

	public Vector3 TintColor = Vector3.one;

	public Vector4 Shadowmask;

	public float Scale = 1f;

	public float Rotation;

	[NonSerialized]
	public uint MortonCode;
}
