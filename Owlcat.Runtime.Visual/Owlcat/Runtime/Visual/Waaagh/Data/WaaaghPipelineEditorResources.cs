using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

public class WaaaghPipelineEditorResources : ScriptableObject
{
	[Serializable]
	[ReloadGroup]
	public sealed class MaterialResources
	{
		[Reload("Runtime/Waaagh/Materials/Lit-default.mat", ReloadAttribute.Package.Root)]
		public Material Lit;

		[Reload("Runtime/Waaagh/Materials/Particles-default.mat", ReloadAttribute.Package.Root)]
		public Material Particles;

		[Reload("Runtime/Waaagh/Materials/Terrain-default.mat", ReloadAttribute.Package.Root)]
		public Material Terrain;

		[Reload("Runtime/Waaagh/Materials/Decal-default.mat", ReloadAttribute.Package.Root)]
		public Material Decal;

		[Reload("Runtime/Waaagh/Materials/UI-default.mat", ReloadAttribute.Package.Root)]
		public Material UI;
	}

	public MaterialResources Materials;
}
