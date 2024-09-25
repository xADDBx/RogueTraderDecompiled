using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[Serializable]
[ReloadGroup]
public class LocalVolumetricFogSettings
{
	[Reload("Shaders/Utils/Texture3DAtlas.compute", ReloadAttribute.Package.Root)]
	public ComputeShader Texture3DAtlasCS;

	public LocalVolumetricFogResolution MaxLocalVolumetricFogSize = LocalVolumetricFogResolution.Resolution32;

	[Range(1f, 512f)]
	public int MaxLocalVolumetricFogOnScreen = 64;

	[Range(1f, 64f)]
	public int MaxTexturesInAtlas = 4;
}
