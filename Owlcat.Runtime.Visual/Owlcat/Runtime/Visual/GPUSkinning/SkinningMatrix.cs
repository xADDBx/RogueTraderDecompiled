using System;
using UnityEngine;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.GPUSkinning;

[Serializable]
[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer, null)]
public struct SkinningMatrix
{
	public Vector4 r0;

	public Vector4 r1;

	public Vector4 r2;
}
