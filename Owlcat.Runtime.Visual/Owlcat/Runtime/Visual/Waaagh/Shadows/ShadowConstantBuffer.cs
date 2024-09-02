using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.225\\Runtime\\Waaagh\\Shadows\\ShadowConstantBuffer.cs", needAccessors = false, generateCBuffer = true)]
public struct ShadowConstantBuffer
{
	public const int MaxShadowEntriesCount = 128;

	public const int MaxShadowFacesCount = 512;

	[HLSLArray(128, typeof(Vector4))]
	public unsafe fixed float _ShadowEntryParameters[512];

	[HLSLArray(512, typeof(Vector4))]
	public unsafe fixed float _ShadowFaceAtlasScaleOffsets[2048];

	[HLSLArray(512, typeof(Vector4))]
	public unsafe fixed float _ShadowFaceSpheres[2048];

	[HLSLArray(512, typeof(Vector4))]
	public unsafe fixed float _ShadowFaceDirections[2048];

	[HLSLArray(512, typeof(Matrix4x4))]
	public unsafe fixed float _ShadowFaceMatrices[8192];
}
