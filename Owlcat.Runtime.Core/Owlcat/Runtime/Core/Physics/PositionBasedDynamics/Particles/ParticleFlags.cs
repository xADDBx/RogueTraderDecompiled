using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

[Flags]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.core@0.0.48\\Runtime\\Physics\\PositionBasedDynamics\\Particles\\ParticleFlags.cs")]
public enum ParticleFlags : uint
{
	None = 0u,
	SkipGlobalGravity = 1u,
	SkipGlobalWind = 2u,
	SkipCollision = 4u
}
