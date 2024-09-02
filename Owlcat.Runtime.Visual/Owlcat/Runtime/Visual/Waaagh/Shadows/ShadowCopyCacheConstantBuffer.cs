using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.225\\Runtime\\Waaagh\\Shadows\\ShadowCopyCacheConstantBuffer.cs", needAccessors = false, generateCBuffer = true)]
[BurstCompile]
public struct ShadowCopyCacheConstantBuffer
{
	public const int MaxShadowEntriesCount = 128;

	[HLSLArray(128, typeof(Vector4))]
	public unsafe fixed float _ShadowDynamicAtlasScaleOffsets[512];

	[HLSLArray(128, typeof(Vector4))]
	public unsafe fixed float _ShadowCachedAtlasScaleOffsets[512];
}
