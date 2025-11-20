using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.BilateralUpsample;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\BilateralUpsample\\BilateralUpsampleConstants.cs", needAccessors = false, generateCBuffer = true)]
internal struct ShaderVariablesBilateralUpsample
{
	public Vector4 _HalfScreenSize;

	[HLSLArray(12, typeof(Vector4))]
	public unsafe fixed float _DistanceBasedWeights[48];

	[HLSLArray(8, typeof(Vector4))]
	public unsafe fixed float _TapOffsets[32];
}
