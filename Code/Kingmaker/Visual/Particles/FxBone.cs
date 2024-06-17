using System;
using System.Linq;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Particles.Blueprints;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

[Serializable]
public class FxBone
{
	[FxBoneName]
	[ArrayElementNameProvider]
	public string Name;

	[Space]
	public string[] Aliases;

	public float ParticleSize = 1f;

	public Vector3 LocalOffset;

	public bool Rotate;

	public float CameraOffset;

	[EnumFlagsAsButtons(ColumnCount = 2)]
	public FxBoneFlags Flags;

	public BlueprintFxLocatorGroup Group { get; set; }

	public Transform Transform { get; set; }

	public void Copy(FxBone original)
	{
		Name = original.Name;
		Aliases = original.Aliases?.ToArray();
		ParticleSize = original.ParticleSize;
		LocalOffset = original.LocalOffset;
		Rotate = original.Rotate;
		CameraOffset = original.CameraOffset;
		Flags = (((original.Flags & FxBoneFlags.Special) != 0) ? FxBoneFlags.Special : ((FxBoneFlags)0));
	}
}
